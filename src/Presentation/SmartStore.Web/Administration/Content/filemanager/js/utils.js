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
var FileTypes = new Array();
FileTypes['image'] = new Array('jpg', 'jpeg', 'png', 'gif');
FileTypes['media'] = new Array('avi', 'flv', 'swf', 'wmv', 'mp3', 'wma', 'mpg', 'mpeg');
FileTypes['document'] = new Array('doc', 'docx', 'txt', 'rtf', 'pdf', 'xls', 'mdb', 'html', 'htm', 'db');

RoxyLang = {
	"CreateDir": "Create",
	"RenameDir": "Rename",
	"DeleteDir": "Delete",
	"AddFile": "Add file",
	"Preview": "Preview",
	"RenameFile": "Rename",
	"DeleteFile": "Delete",
	"SelectFile": "Select",
	"OrderBy": "Order by",
	"Name_asc": "&uarr;&nbsp;&nbsp;Name",
	"Size_asc": "&uarr;&nbsp;&nbsp;Size",
	"Date_asc": "&uarr;&nbsp;&nbsp;Date",
	"Name_desc": "&darr;&nbsp;&nbsp;Name",
	"Size_desc": "&darr;&nbsp;&nbsp;Size",
	"Date_desc": "&darr;&nbsp;&nbsp;Date",
	"Name": "Name",
	"Size": "Size",
	"Date": "Date",
	"Dimensions": "Dimensions",
	"Cancel": "Cancel",
	"LoadingDirectories": "Loading folders...",
	"LoadingFiles": "Loading files...",
	"DirIsEmpty": "This folder is empty",
	"NoFilesFound": "No files found",
	"Upload": "Upload",
	"T_CreateDir": "Create new folder",
	"T_RenameDir": "Rename folder",
	"T_DeleteDir": "Delete selected folder",
	"T_AddFile": "Upload files",
	"T_Preview": "Preview selected file",
	"T_RenameFile": "Rename file",
	"T_DeleteFile": "Delete file",
	"T_SelectFile": "Select highlighted file",
	"T_ListView": "List view",
	"T_ThumbsView": "Thumbnails view",
	"Q_DeleteFolder": "Delete selected directory?",
	"Q_DeleteFile": "Delete selected file?",
	"E_LoadingConf": "Error loading configuration",
	"E_ActionDisabled": "This action is disabled",
	"E_LoadingAjax": "Error loading",
	"E_MissingDirName": "Missing folder name",
	"E_SelectFiles": "Select files to upload",
	"E_CannotRenameRoot": "Cannot rename root folder.",
	"E_NoFileSelected": "No file selected.",
	"E_CreateDirFailed": "Error creating directory",
	"E_CreateDirInvalidPath": "Cannot create directory - path doesn't exist",
	"E_CannotDeleteDir": "Error deleting directory",
	"E_DeleteDirInvalidPath": "Cannot delete directory - path doesn't exist",
	"E_DeletеFile": "Error deleting file",
	"E_DeleteFileInvalidPath": "Cannot delete file - path doesn't exist",
	"E_DeleteNonEmpty": "Cannot delete - folder is not empty",
	"E_CannotMoveDirToChild": "Cannot move directory to its subdirectory",
	"E_DirAlreadyExists": "Directory with the same name already exists",
	"E_MoveDir": "Error moving directory",
	"E_MoveDirInvalisPath": "Cannot move directory - directory doesn't exist",
	"E_MoveFile": "Error moving file",
	"E_MoveFileInvalisPath": "Cannot move file - file doesn't exist",
	"E_MoveFileAlreadyExists": "File with the same name already exists",
	"E_RenameDir": "Error renaming directory",
	"E_RenameDirInvalidPath": "Cannot rename directory - path doesn't exist",
	"E_RenameFile": "Error renaming file",
	"E_RenameFileInvalidPath": "Cannot rename file - file doesn't exist",
	"E_UploadNotAll": "There is and error uploading some files.",
	"E_UploadNoFiles": "There are no files to upload or file is too big.",
	"E_UploadInvalidPath": "Cannot upload files - path doesn't exist",
	"E_FileExtensionForbidden": "This type of file cannot be handled - invalid file extension.",
	"DownloadFile": "Download",
	"T_DownloadFile": "Download file",
	"E_CannotDeleteRoot": "Cannot delete root folder",
	"file": "file",
	"files": "files",
	"Cut": "Cut",
	"Copy": "Copy",
	"Paste": "Paste",
	"E_CopyFile": "Error copying file",
	"E_CopyFileInvalisPath": "Cannot copy file - path doesn't exist",
	"E_CopyDirInvalidPath": "Cannot copy directory - path doesn't exist",
	"E_CreateArchive": "Error creating zip archive.",
	"E_UploadingFile": "error"
};

var RoxyIconHints = {
	"pdf": { name: "far fa-file-pdf", color: "#F44336" },
	"document": { name: "far fa-file-word", color: "#2B579A" },
	"spreadsheet": { name: "far fa-file-excel", color: "#217346" },
	"database": { name: "fa fa-database", color: "#3ba074" },
	"presentation": { name: "far fa-file-powerpoint", color: "#D24726" },
	"archive": { name: "far fa-file-archive", color: "#3F51B5" },
	"audio": { name: "far fa-file-audio", color: "#009688" },
	"markup": { name: "far fa-file-code", color: "#4CAF50" },
	"code": { name: "fa fa-bolt", color: "#4CAF50" },
	"exe": { name: "fa fa-cog", color: "#58595B" },
	"image": { name: "far fa-file-image", color: "#e77c00" },
	"text": { name: "far fa-file-alt", color: "#607D8B" },
	"video": { name: "far fa-file-video", color: "#FF5722" },
	"font": { name: "fa fa-font", color: "#797985" },
	"misc": { name: "far fa-file", color: "#ccc" }
};

function RoxyUtils() { }
RoxyUtils.GetRootPath = function (path) {
	return roxy_root + path;
};

RoxyUtils.GetAssetPath = function (path) {
	return roxy_assets_root + path;
};

RoxyUtils.FixPath = function (path) {
	if (!path)
		return '';
	var ret = path.replace(/\\/g, '');
	ret = ret.replace(/\/\//g, '/');
	ret = ret.replace(':/', '://');

	return ret;
};

RoxyUtils.FormatDate = function (date) {
	var ret = '';
	try {
		ret = $.format.date(date, RoxyFilemanConf.DATEFORMAT);
	} catch (ex) {
		//alert(ex);
		ret = date.toString();
		ret = ret.substr(0, ret.indexOf('UTC'));
	}
	return ret;
};

RoxyUtils.GetPath = function (path) {
	var ret = '';
	path = RoxyUtils.FixPath(path);
	if (path.indexOf('/') > -1)
		ret = path.substring(0, path.lastIndexOf('/'));

	return ret;
};

RoxyUtils.GetUrlParam = function (varName, url) {
	var ret = '';
	if (!url)
		url = self.location.href;
	if (url.indexOf('?') > -1) {
		url = url.substr(url.indexOf('?') + 1);
		url = url.split('&');
		for (i = 0; i < url.length; i++) {
			var tmp = url[i].split('=');
			if (tmp[0] && tmp[1] && tmp[0] === varName) {
				ret = tmp[1];
				break;
			}
		}
	}

	return ret;
};

RoxyUtils.GetFilename = function (path) {
	var ret = path;
	path = RoxyUtils.FixPath(path);
	if (path.indexOf('/') > -1) {
		ret = path.substring(path.lastIndexOf('/') + 1);
	}

	return ret;
};

RoxyUtils.MakePath = function () {
	ret = '';
	if (arguments && arguments.length > 0) {
		for (var i = 0; i < arguments.length; i++) {
			ret += ($.isArray(arguments[i]) ? arguments[i].join('/') : arguments[i]);
			if (i < (arguments.length - 1))
				ret += '/';
		}
		ret = RoxyUtils.FixPath(ret);
	}

	return ret;
};

RoxyUtils.GetFileExt = function (path) {
	var ret = '';
	path = RoxyUtils.GetFilename(path);
	if (path.indexOf('.') > -1) {
		ret = path.substring(path.lastIndexOf('.') + 1);
	}

	return ret;
};

RoxyUtils.FileExists = function (path) {
	var ret = false;

	$.ajax({
		url: path,
		type: 'HEAD',
		async: false,
		dataType: 'text',
		success: function () {
			ret = true;
		}
	});

	return ret;
};

RoxyUtils.GetFileIcon = function (path) {
	ret = 'images/filetypes/unknown.png'; //'images/filetypes/file_extension_' + RoxyUtils.GetFileExt(path).toLowerCase() + '.png';
	if (fileTypeIcons[RoxyUtils.GetFileExt(path).toLowerCase()]) {
		ret = 'images/filetypes/' + fileTypeIcons[RoxyUtils.GetFileExt(path).toLowerCase()];
	}

	return RoxyUtils.GetAssetPath(ret);
};

RoxyUtils.GetFileSize = function (path) {
	var ret = 0;
	$.ajax({
		url: path,
		type: 'HEAD',
		async: false,
		success: function (d, s, xhr) {
			ret = xhr.getResponseHeader('Content-Length');
		}
	});
	if (!ret)
		ret = 0;

	return ret;
};

RoxyUtils.GetFileType = function (path, mime) {
	var ext = RoxyUtils.GetFileExt(path).toLowerCase();
	var ret;

	switch (ext) {
		case "pdf":
			ret = "pdf";
			break;
		case "doc":
		case "docx":
		case "docm":
		case "odt":
		case "dot":
		case "rtf":
		case "dotx":
		case "dotm":
			ret = "document";
			break;
		case "mdb":
		case "db":
		case "sqlite":
			ret = "database";
			break;
		case "xls":
		case "xlsx":
		case "xlsm":
		case "xlsb":
		case "ods":
		case "csv":
			ret = "spreadsheet";
			break;
		case "ppt":
		case "pptx":
		case "pptm":
		case "ppsx":
		case "odp":
		case "potx":
		case "pot":
		case "potm":
		case "pps":
		case "ppsm":
			ret = "presentation";
			break;
		case "zip":
		case "rar":
		case "7z":
			ret = "archive";
			break;
		case "png":
		case "jpg":
		case "jpeg":
		case "bmp":
		case "gif":
        case "webp":
        case "svg":
		case "psd":
			ret = "image";
			break;
		case "mp3":
		case "wav":
		case "ogg":
		case "wma":
			ret = "audio";
			break;
		case "mp4":
		case "mkv":
		case "wmv":
		case "avi":
		case "asf":
		case "mpg":
		case "mpeg":
		case "flv":
		case "swf":
			ret = "video";
			break;
		case "txt":
		case "css":
			ret = "text";
			break;
		case "exe":
			ret = "exe";
			break;
		case "ttf":
		case "eot":
		case "woff":
		case "woff2":
			ret = "font";
			break;
		case "xml":
		case "html":
		case "htm":
			ret = "markup";
			break;
		case "js":
		case "json":
			ret = "code";
			break;
		default:
			ret = "misc";
	}

	if (mime && ret === "misc") {
		mime = mime.substring(0, mime.indexOf("/"));
		if (RoxyIconHints[mime]) {
			ret = mime;
		}
	}

	return ret;
};

RoxyUtils.IsImage = function (path) {
	return RoxyUtils.GetFileType(path) === 'image';
};

RoxyUtils.FormatFileSize = function (x) {
	var suffix = 'B';
	if (!x)
		x = 0;
	if (x > 1024) {
		x = x / 1024;
		suffix = 'KB';
	}
	if (x > 1024) {
		x = x / 1024;
		suffix = 'MB';
	}
	x = new Number(x);
	return x.toFixed(2) + ' ' + suffix;
};

RoxyUtils.AddParam = function (url, n, v) {
	url += (url.indexOf('?') > -1 ? '&' : '?') + n + '=' + encodeURIComponent(v);

	return url;
};

RoxyUtils.SelectText = function (field_id, start, end) {
	try {
		var field = document.getElementById(field_id);
		if (field.createTextRange) {
			var selRange = field.createTextRange();
			selRange.collapse(true);
			selRange.moveStart('character', start);
			selRange.moveEnd('character', end - start);
			selRange.select();
		} else if (field.setSelectionRange) {
			field.setSelectionRange(start, end);
		} else if (field.selectionStart) {
			field.selectionStart = start;
			field.selectionEnd = end;
		}
		field.focus();
	} catch (ex) { /**/ }
};

RoxyFilemanConf = {};
RoxyUtils.LoadConfig = function () {
	$.ajax({
		url: RoxyUtils.GetAssetPath('conf.json'),
		dataType: 'json',
		async: false,
		success: function (data) {
			RoxyFilemanConf = data;
		},
		error: function (data) {
			alert(t('E_LoadingConf'));
		}
	});
};

RoxyUtils.Translate = function () {
	$('[data-lang-t]').each(function () {
		var key = $(this).attr('data-lang-t');
		$(this).prop('title', t(key));
	});
	$('[data-lang-v]').each(function () {
		var btn = $(this);
		var key = btn.attr('data-lang-v');
		if (btn.is('button')) {
			btn.find('> span').text(t(key));
		}
		else {
			btn.prop('value', t(key));
		}	
	});
	$('[data-lang]').each(function () {
		var el = $(this);
		var key = el.attr('data-lang');
		if (el.is('.dropdown-item')) {
			el.find('> span').text(t(key));
		}
		else {
			el.html(t(key));
		}
	});
};

RoxyUtils.GetCookies = function () {
	var ret = new Object();
	var tmp = document.cookie.replace(' ', '');
	tmp = tmp.split(';');

	for (i in tmp) {
		var s = tmp[i].split('=');
		if (s.length > 1) {
			ret[$.trim(s[0].toString())] = decodeURIComponent($.trim(s[1].toString())) || '';
		}
	}

	return ret;
};

RoxyUtils.GetCookie = function (key) {
	var tmp = RoxyUtils.GetCookies();

	return tmp[key] || '';
};

RoxyUtils.SetCookie = function (key, val, hours, path) {
	var expires = new Date();
	if (hours) {
		expires.setTime(expires.getTime() + (hours * 3600 * 1000));
	}

	if (!path) {
		path = '/';
	}

	document.cookie = key + '=' + encodeURIComponent(val) + '; path=' + path + (hours ? '; expires=' + expires.toGMTString() : '');
};

RoxyUtils.ToBool = function (val) {
	var ret = false;
	val = val.toString().toLowerCase();
	if (val === 'true' || val === 'on' || val === 'yes' || val === '1')
		ret = true;

	return ret;
};

RoxyUtils.UnsetCookie = function (key) {
	document.cookie = key + "=; expires=Thu, 01 Jan 1972 00:00:00 UTC";
};

function t(tag) {
	var ret = tag;
	if (RoxyLang && RoxyLang[tag])
		ret = RoxyLang[tag];
	return ret;
}