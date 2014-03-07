using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Migrations;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using SmartStore.Core.Data;
using SmartStore.Utilities;

namespace SmartStore.Data.Initializers
{

	/// <summary>
	///     An implementation of <see cref="IDatabaseInitializer{TContext}" /> that will use Code First Migrations
	///     to update the database to the latest version.
	/// </summary>
	public class MigrateDatabaseToLatestVersionEx<TContext, TConfig> : IDatabaseInitializer<TContext> 
		where TContext : DbContext
		where TConfig : DbMigrationsConfiguration<TContext>, new()
	{
		private readonly DataSettings _dataSettings;
		private readonly string[] _sqlFiles;
		private DbMigrationsConfiguration _config;

		public MigrateDatabaseToLatestVersionEx()
		{
		}

		public MigrateDatabaseToLatestVersionEx(DataSettings dataSettings)
		{
			Guard.ArgumentNotNull(() => dataSettings);
			this._dataSettings = dataSettings;
		}

		public MigrateDatabaseToLatestVersionEx(string[] sqlFiles)
		{
			Guard.ArgumentNotNull(() => sqlFiles);
			this._sqlFiles = sqlFiles;
		}

		public MigrateDatabaseToLatestVersionEx(DataSettings dataSettings, string[] sqlFiles)
		{
			Guard.ArgumentNotNull(() => dataSettings);
			Guard.ArgumentNotNull(() => sqlFiles);
			this._dataSettings = dataSettings;
			this._sqlFiles = sqlFiles;
		}
		
		/// <summary>
		/// Initializes the database.
		/// </summary>
		/// <param name="context">The context.</param>
		/// <inheritdoc />
		public void InitializeDatabase(TContext context)
		{
			if (_config == null)
			{
				_config = new TConfig();
				if (_dataSettings != null && _dataSettings.IsValid())
				{
					_config.TargetDatabase = new DbConnectionInfo(_dataSettings.DataConnectionString, _dataSettings.ProviderInvariantName);
				}
			}

			var newDb = !context.Database.Exists();

			var migrator = new DbMigrator(_config);
			if (!newDb)
			{
				var local = migrator.GetLocalMigrations();
				var pending = migrator.GetPendingMigrations();
				if (local.Count() == pending.Count())
				{
					newDb = true;
				}
			}

			//var compat = context.Database.CompatibleWithModel(false);
			//bool compat2 = false;
			//try
			//{
			//	compat2 = context.Database.CompatibleWithModel(true);
			//}
			//catch { }

			// create or migrate the database now
			migrator.Update();

			if (newDb)
			{
				// advanced db init
				RunSqlFiles(context);

				// seed data
				Seed(context);
			}
		}


		/// <summary>
		/// Seeds the specified context.
		/// </summary>
		/// <param name="context">The context.</param>
		protected virtual void Seed(TContext context)
		{
		}

		#region Sql File Handling

		private void RunSqlFiles(TContext context)
		{
			if (_sqlFiles == null || _sqlFiles.Length == 0)
				return;

			foreach (var file in _sqlFiles)
			{
				using (var reader = ReadSqlFile(file))
				{
					foreach (var cmd in ParseCommands(reader))
					{
						if (cmd.HasValue())
						{
							context.Database.ExecuteSqlCommand(cmd);
						}
					}
				}
			}
		}

		protected virtual StreamReader ReadSqlFile(string fileName)
		{
			Guard.ArgumentNotEmpty(() => fileName);

			if (fileName.StartsWith("~") || fileName.StartsWith("/"))
			{
				string path = CommonHelper.MapPath(fileName);
				if (!File.Exists(path)) 
				{
					return StreamReader.Null;
				}

				return new StreamReader(File.OpenRead(path));
			}

			// SQL file is obviously an embedded resource
			// TODO: (MC) add support for assemblies other than SmartStore.Data
			var asm = Assembly.GetExecutingAssembly();
			var asmName = asm.FullName.Substring(0, asm.FullName.IndexOf(','));
			var name = String.Format("{0}.Sql.{1}",
				asmName,
				fileName);
			var stream = asm.GetManifestResourceStream(name);
			Debug.Assert(stream != null);
			return new StreamReader(stream);
		}

		private IEnumerable<string> ParseCommands(TextReader reader)
		{
			var statement = string.Empty;
			while ((statement = ReadNextStatement(reader)) != null)
			{
				yield return statement;
			}
		}

		private string ReadNextStatement(TextReader reader)
		{
			var sb = new StringBuilder();

			string lineOfText;

			while (true)
			{
				lineOfText = reader.ReadLine();
				if (lineOfText == null)
				{
					if (sb.Length > 0)
						return sb.ToString();
					else
						return null;
				}

				if (lineOfText.TrimEnd().ToUpper() == "GO")
					break;

				sb.Append(lineOfText + Environment.NewLine);
			}

			return sb.ToString();
		}

		#endregion

	}

}
