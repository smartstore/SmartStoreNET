/*
  RoxyFileman - web based file manager. Ready to use with CKEditor, TinyMCE. 
  Can be easily integrated with any other WYSIWYG editor or CMS.

  Copyright (C) 2013, RoxyFileman.com - Lyubomir Arsov. All rights reserved.
  For licensing, see LICENSE.txt or http://RoxyFileman.com/license

  This program is free software: you can redistribute it and/or modify
  it under the terms of the GNU General Public License as published by
  the Free Software Foundation, either version 3 of the License.

  This program is distributed in the hope that it will be useful,
  but WITHOUT ANY WARRANTY; without even the implied warranty of
  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
  GNU General Public License for more details.

  You should have received a copy of the GNU General Public License
  along with this program.  If not, see <http://www.gnu.org/licenses/>.

  Contact: Lyubomir Arsov, liubo (at) web-lobby.com
*/

$(function () {
	$('#pnlDirList').on('contextmenu', '.dir-item', function (e) {
		e.stopPropagation();
		e.preventDefault();
		closeMenus('file');
		selectDir(this, true);
		var t = e.pageY;
		var menuEnd = t + $('#menuDir').height() + 30;
		if (menuEnd > $(window).height()) {
			offset = menuEnd - $(window).height() + 30;
			t -= offset;
		}
		if (t < 0)
			t = 0;
		$('#menuDir').css({
			top: t + 'px',
			left: e.pageX + 'px'
		}).show();

		return false;
	});

	$('#pnlDirList').on('click', '.dir-item', function (e) {
		e.preventDefault();
		selectDir(this);
	});

	$('#pnlDirList').on('click', '.dirPlus', function (e) {
		e.stopPropagation();
		e.preventDefault();
		var d = Directory.Parse($(this).closest('li').attr('data-path'));
		d.Expand();
	});
});

function Directory(fullPath, numDirs, numFiles) {
	if (!fullPath) fullPath = '';
	this.fullPath = fullPath;
	this.name = RoxyUtils.GetFilename(fullPath);
	if (!this.name)
		this.name = 'My files';
	this.path = RoxyUtils.GetPath(fullPath);
	this.dirs = (numDirs ? numDirs : 0);
	this.files = (numFiles ? numFiles : 0);
	this.filesList = [];

	this.Show = function () {
		var html = this.GetHtml();
		var el = null;
		el = $('li[data-path="' + this.path + '"]');
		if (el.length == 0)
			el = $('#pnlDirList');
		else {
			if (el.children('ul').length == 0)
				el.append('<ul></ul>');
			el = el.children('ul');
		}
		if (el) {
			el.append(html);
			this.SetEvents();
		}
	};
	this.SetEvents = function () {
		var el = this.GetElement();
		el.draggable({
			helper: makeDragDir,
			start: startDragDir,
			cursorAt: {
				left: 10,
				top: 10
			},
			delay: 200
		});
		el.find('> .dir-item').droppable({
			drop: moveObject,
			over: dragFileOver,
			out: dragFileOut
		});
	};
	this.GetHtml = function () {
		var dirClass = (this.dirs > 0 ? "" : " invisible");

		var html = '<li data-path="' + this.fullPath + '" data-dirs="' + this.dirs + '" data-files="' + this.files + '" class="directory">';
		html += '<div class="d-flex flex-row flex-nowrap align-items-center dir-item"><i class="fa fa-chevron-right dirPlus' + dirClass + '"></i>';
		html += '<img src="' + RoxyUtils.GetAssetPath("images/folder.png") + '" class="dir mr-1"><span class="name">' + this.name + (parseInt(this.files) ? ' (' + this.files + ')' : '') + '</span></div>';
		html += '</li>';
		
		return html;
	};
	this.SetStatusBar = function () {
		$('#pnlStatus').html(this.files + ' ' + (this.files == 1 ? t('file') : t('files')));
	};
	this.SetSelectedFile = function (path) {
		if (path) {
			var f = File.Parse(path);
			if (f) {
				selectFile(f.GetElement());
			}
		}
	};
	this.Select = function (selectedFile, indeterm) {
		var li = this.GetElement();
		var dir = li.find('> .dir-item');
		var currentSelected = getSelectedDir();

		if (indeterm && currentSelected) {
			if (currentSelected.fullPath != li.data('path')) {
				$('#pnlDirList').data('indeterm', dir);
				$('#pnlDirList .indeterm').removeClass('indeterm');
				dir.addClass('indeterm');
			}
		}
		else {
			dir.addClass('selected');
			$('#pnlDirList li[data-path!="' + this.fullPath + '"] > .dir-item').removeClass('selected');
			this.SetStatusBar();

			var p = this.GetParent();
			while (p) {
				p.Expand(true);
				p = p.GetParent();
			}
			this.ListFiles(true, selectedFile);
			setLastDir(this.fullPath);
		}		
	};
	this.GetElement = function () {
		return $('li[data-path="' + this.fullPath + '"]');
	};
	this.IsExpanded = function () {
		var el = this.GetElement().children('ul');
		return (el && el.is(":visible"));
	};
	this.IsIndeterm = function () {
		var el = this.GetElement().find('> .dir-item');
		return el.is(".indeterm");
	};
	this.IsListed = function () {
		if ($('#hdDir').val() == this.fullPath)
			return true;
		return false;
	};
	this.GetExpanded = function (el) {
		var ret = new Array();
		if (!el)
			el = $('#pnlDirList');
		el.children('li').each(function () {
			var path = $(this).attr('data-path');
			var d = new Directory(path);
			if (d) {
				if (d.IsExpanded() && path)
					ret.push(path);
				ret = ret.concat(d.GetExpanded(d.GetElement().children('ul')));
			}
		});

		return ret;
	};
	this.RestoreExpanded = function (expandedDirs) {
		for (i = 0; i < expandedDirs.length; i++) {
			var d = Directory.Parse(expandedDirs[i]);
			if (d)
				d.Expand(true);
		}
	};
	this.GetParent = function () {
		return Directory.Parse(this.path);
	};
	this.SetOpened = function () {
		var li = this.GetElement();
		var chevrons = li.children('div').children('.dirPlus');
		if (li.find('li').length < 1)
			chevrons.addClass('invisible');
		else if (this.IsExpanded())
			chevrons.removeClass('invisible fa-chevron-right').addClass("fa-chevron-down");
		else
			chevrons.removeClass('invisible fa-chevron-down').addClass("fa-chevron-right");
	};
	this.Update = function (newPath) {
		var el = this.GetElement();
		if (newPath) {
			this.fullPath = newPath;
			this.name = RoxyUtils.GetFilename(newPath);
			if (!this.name)
				this.name = 'My files';
			this.path = RoxyUtils.GetPath(newPath);
		}
		el.data('path', this.fullPath);
		el.data('dirs', this.dirs);
		el.data('files', this.files);
		el.children('div').children('.name').html(this.name + ' (' + this.files + ')');
		this.SetOpened();
	};
	this.LoadAll = function (selectedDir) {
		var expanded = this.GetExpanded();
		var dirListURL = RoxyUtils.GetRootPath(RoxyFilemanConf.DIRLIST);
		if (!dirListURL) {
			alert(t('E_ActionDisabled'));
			return;
		}
		$('#pnlLoadingDirs').show();
		$('#pnlDirList').hide();
		dirListURL = RoxyUtils.AddParam(dirListURL, 'type', RoxyUtils.GetUrlParam('type'));

		var dir = this;
		$.ajax({
			url: dirListURL,
			type: 'POST',
			dataType: 'json',
			async: false,
			cache: false,
			success: function (dirs) {
				$('#pnlDirList').children('li').remove();
				var d;
				for (i = 0; i < dirs.length; i++) {
					d = new Directory(dirs[i].p, dirs[i].d, dirs[i].f);
					d.Show();
				}
				$('#pnlLoadingDirs').hide();
				$('#pnlDirList').show();
				dir.RestoreExpanded(expanded);
				d = Directory.Parse(selectedDir);
				if (d) d.Select();
			},
			error: function (data) {
				$('#pnlLoadingDirs').hide();
				$('#pnlDirList').show();
				alert(t('E_LoadingAjax') + ' ' + RoxyFilemanConf.DIRLIST);
			}
		});
	};
	this.Expand = function (show) {
		var li = this.GetElement();
		var el = li.children('ul');
		if (this.IsExpanded() && !show)
			el.hide();
		else
			el.show();

		this.SetOpened();
	};
	this.Create = function (newName) {
		if (!newName)
			return false;
		else if (!RoxyFilemanConf.CREATEDIR) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.CREATEDIR), 'd', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newName);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				d: this.fullPath,
				n: newName
			},
			dataType: 'json',
			async: false,
			cache: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					item.LoadAll(RoxyUtils.MakePath(item.fullPath, newName));
					ret = true;
				} else {
					alert(data.msg);
				}
			},
			error: function (data) {
				alert(t('E_LoadingAjax') + ' ' + item.name);
			}
		});
		return ret;
	};
	this.Delete = function () {
		if (!RoxyFilemanConf.DELETEDIR) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.DELETEDIR), 'd', this.fullPath);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				d: this.fullPath
			},
			dataType: 'json',
			async: false,
			cache: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					var parent = item.GetParent();
					parent.dirs--;
					parent.Update();
					parent.Select();
					item.GetElement().remove();
					ret = true;
				}
				if (data.msg)
					alert(data.msg);
			},
			error: function (data) {
				alert(t('E_LoadingAjax') + ' ' + item.name);
			}
		});
		return ret;
	};
	this.Rename = function (newName) {
		if (!newName)
			return false;
		else if (!RoxyFilemanConf.RENAMEDIR) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.RENAMEDIR), 'd', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newName);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				d: this.fullPath,
				n: newName
			},
			dataType: 'json',
			async: false,
			cache: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					var newPath = RoxyUtils.MakePath(item.path, newName);
					item.Update(newPath);
					item.Select();
					ret = true;
				}
				if (data.msg)
					alert(data.msg);
			},
			error: function (data) {
				alert(t('E_LoadingAjax') + ' ' + item.name);
			}
		});
		return ret;
	};
	this.Copy = function (newPath) {
		if (!RoxyFilemanConf.COPYDIR) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.COPYDIR), 'd', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newPath);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				d: this.fullPath,
				n: newPath
			},
			dataType: 'json',
			async: false,
			cache: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					var d = Directory.Parse(newPath);
					if (d) {
						d.LoadAll(d.fullPath);
					}
					ret = true;
				}
				if (data.msg)
					alert(data.msg);
			},
			error: function (data) {
				alert(t('E_LoadingAjax') + ' ' + url);
			}
		});
		return ret;
	};
	this.Move = function (newPath) {
		if (!newPath)
			return false;
		else if (!RoxyFilemanConf.MOVEDIR) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.MOVEDIR), 'd', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newPath);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				d: this.fullPath,
				n: newPath
			},
			dataType: 'json',
			async: false,
			cache: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					item.LoadAll(RoxyUtils.MakePath(newPath, item.name));
					ret = true;
				}
				if (data.msg)
					alert(data.msg);
			},
			error: function (data) {
				alert(t('E_LoadingAjax') + ' ' + item.name);
			}
		});
		return ret;
	};
	this.ListFiles = function (refresh, selectedFile) {
		$('#pnlLoading').show();
		$('#pnlEmptyDir').hide();
		$('#pnlFileList').hide();
		$('#pnlSearchNoFiles').hide();
		this.LoadFiles(refresh, selectedFile);
	};
	this.FilesLoaded = function (filesList, selectedFile) {
		var list = $('#pnlFileList');
		filesList = this.SortFiles(filesList);
		
		var html = [];
		for (i = 0; i < filesList.length; i++) {
			var f = filesList[i];
			html.push(f.GenerateHtml());
		}

		// Set Html
		list.html(html.join(""));

		// Bind events
		list.find('.file-item').tooltip({
			show: {
				delay: 700,
				duration: 100
			},
			hide: 200,
			track: true,
			content: tooltipContent
		});

		$('#hdDir').val(this.fullPath);
		$('#pnlLoading').hide();
		var liLen = list.children('li').length;
		if (liLen == 0)
			$('#pnlEmptyDir').show();
		this.files = liLen;
		this.Update();
		this.SetStatusBar();
		filterFiles();
		switchView();
		list.show();
		this.SetSelectedFile(selectedFile);
	};
	this.LoadFiles = function (refresh, selectedFile) {
		if (!RoxyFilemanConf.FILESLIST) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var ret = new Array();
		var fileURL = RoxyUtils.GetRootPath(RoxyFilemanConf.FILESLIST);
		fileURL = RoxyUtils.AddParam(fileURL, 'd', this.fullPath);
		fileURL = RoxyUtils.AddParam(fileURL, 'type', RoxyUtils.GetUrlParam('type'));
		var item = this;
		if (!this.IsListed() || refresh) {
			$.ajax({
				url: fileURL,
				type: 'POST',
				data: {
					d: this.fullPath,
					type: RoxyUtils.GetUrlParam('type')
				},
				dataType: 'json',
				async: true,
				cache: false,
				success: function (files) {
					for (i = 0; i < files.length; i++) {
						var f = files[i];
						ret.push(new File(f.p, f.s, f.t, f.w, f.h, f.m));
					}
					item.FilesLoaded(ret, selectedFile);
				},
				error: function (data) {
					alert(t('E_LoadingAjax') + ' ' + fileURL);
				}
			});
		} else {
			$('#pnlFileList li').each(function () {
				ret.push(new File($(this).attr('data-path'), $(this).attr('data-size'), $(this).attr('data-time'), $(this).attr('data-w'), $(this).attr('data-h')));
			});
			item.FilesLoaded(ret, selectedFile);
		}

		return ret;
	};

	this.SortByName = function (files, order) {
		files.sort(function (a, b) {
			var x = (order == 'desc' ? 0 : 2)
			a = a.name.toLowerCase();
			b = b.name.toLowerCase();
			if (a > b)
				return -1 + x;
			else if (a < b)
				return 1 - x;
			else
				return 0;
		});

		return files;
	};
	this.SortBySize = function (files, order) {
		files.sort(function (a, b) {
			var x = (order == 'desc' ? 0 : 2)
			a = parseInt(a.size);
			b = parseInt(b.size);
			if (a > b)
				return -1 + x;
			else if (a < b)
				return 1 - x;
			else
				return 0;
		});

		return files;
	};
	this.SortByTime = function (files, order) {
		files.sort(function (a, b) {
			var x = (order == 'desc' ? 0 : 2)
			a = parseInt(a.time);
			b = parseInt(b.time);
			if (a > b)
				return -1 + x;
			else if (a < b)
				return 1 - x;
			else
				return 0;
		});

		return files;
	};
	this.SortFiles = function (files) {
		var order = $('#ddlOrder').val();
		if (!order)
			order = 'name';

		switch (order) {
			case 'size':
				files = this.SortBySize(files, 'asc');
				break;
			case 'size_desc':
				files = this.SortBySize(files, 'desc');
				break;
			case 'time':
				files = this.SortByTime(files, 'asc');
				break;
			case 'time_desc':
				files = this.SortByTime(files, 'desc');
				break;
			case 'name_desc':
				files = this.SortByName(files, 'desc');
				break;
			default:
				files = this.SortByName(files, 'asc');
		}

		return files;
	};
}
Directory.Parse = function (path) {
	var ret = false;
	var li = $('#pnlDirList').find('li[data-path="' + path + '"]');
	if (li.length > 0)
		ret = new Directory(li.attr('data-path'), li.attr('data-dirs'), li.attr('data-files'));

	return ret;
};