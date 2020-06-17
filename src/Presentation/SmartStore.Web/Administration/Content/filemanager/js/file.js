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
	$(document).on('dblclick', '.file-item', function (e) {
		e.stopPropagation();
		e.preventDefault();
		selectFile(this);
		setFile();
	});

	$('#pnlFileList').on('contextmenu', '.file-item', function (e) {
		e.stopPropagation();
		e.preventDefault();
		closeMenus('dir');
		selectFile(this);
		$(this).tooltip('close');
		var t = e.pageY;
		var menuEnd = t + $('#menuFile').height() + 30;
		if (menuEnd > $(window).height()) {
			offset = menuEnd - $(window).height() + 30;
			t -= offset;
		}
		$('#menuFile').css({
			top: t + 'px',
			left: e.pageX + 'px'
		}).show();

		return false;
	});

	$(document).on('click', '.file-item', function (e) {
		e.stopPropagation();
		e.preventDefault();

		selectFile(this);
	});

	$(document).on('mouseenter', '.file-item', function (e) {
		var li = $(this);

		// create draggable
		if (!li.data('ui-draggable')) {
			li.draggable({
				helper: makeDragFile,
				start: startDragFile,
				addClasses: false,
				appendTo: 'body',
				cursorAt: {
					left: 10,
					top: 10
				},
				delay: 200
			});
		}
	});
});

function File(filePath, fileSize, modTime, w, h, mime) {
	this.fullPath = filePath;
	this.mime = mime;
	this.type = RoxyUtils.GetFileType(filePath, mime);
	this.icon = RoxyIconHints[this.type];
	this.name = RoxyUtils.GetFilename(filePath);
	this.ext = RoxyUtils.GetFileExt(filePath);
	this.path = RoxyUtils.GetPath(filePath);
	this.image = filePath;
	this.size = (fileSize ? fileSize : RoxyUtils.GetFileSize(filePath));
	this.time = modTime;
	this.width = (w ? w : 0);
	this.height = (h ? h : 0);
	this.thumb = this.type === 'image' ? filePath : RoxyUtils.GetAssetPath("images/blank.gif");
	this.GenerateHtml = function () {
		var attrs = [
			'data-mime="' + this.mime + '"',
			'data-path="' + this.fullPath + '"',
			'data-time="' + this.time + '"',
			'data-w="' + this.width + '"',
			'data-h="' + this.height + '"',
			'data-size="' + this.size + '"',
			'title="' + this.name + '"'
		];
		var html = [
			'<li class="file-item' + (this.IsImage() ? ' file-image loaded' : '') + '" ' + attrs.join(' ') + '>',
			'<div class="icon"><img class="thumb" src="' + this.thumb + '"><i class="file-icon fa-fw ' + this.icon.name + '" style="color:' + this.icon.color + '"></i></div>',
			'<span class="time">' + RoxyUtils.FormatDate(new Date(this.time * 1000)) + '</span>',
			'<span class="name">' + this.name + '</span>',
			'<span class="size">' + RoxyUtils.FormatFileSize(this.size) + '</span>',
			'</li>'
		].join("");

		return html;
	};
	this.GetElement = function () {
		return $('li[data-path="' + this.fullPath + '"]');
	};
	this.IsImage = function () {
		return this.type === 'image';
	};
	this.Delete = function () {
		if (!RoxyFilemanConf.DELETEFILE) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var deleteUrl = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.DELETEFILE), 'f', this.fullPath);
		var item = this;
		$.ajax({
			url: deleteUrl,
			type: 'POST',
			data: {
				f: this.fullPath
			},
			dataType: 'json',
			async: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					$('li[data-path="' + item.fullPath + '"]').remove();
					var d = Directory.Parse(item.path);
					if (d) {
						d.files--;
						d.Update();
						d.SetStatusBar();
					}
				} else {
					alert(data.msg);
				}
			},
			error: function (data) {
				alert(t('E_LoadingAjax') + ' ' + deleteUrl);
			}
		});
	};
	this.Rename = function (newName) {
		if (!RoxyFilemanConf.RENAMEFILE) {
			alert(t('E_ActionDisabled'));
			return false;
		}
		if (!newName)
			return false;
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.RENAMEFILE), 'f', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newName);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				f: this.fullPath,
				n: newName
			},
			dataType: 'json',
			async: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					var newPath = RoxyUtils.MakePath(this.path, newName);
					var fileType = RoxyUtils.GetFileIcon(newName);
					var icon = RoxyIconHints[fileType];
					var li = $('li[data-path="' + item.fullPath + '"]');
					li.toggleClass('file-image', fileType == 'image');
					$('.file-icon', li).attr('class', "").addClass('file-icon fa-fw ' + icon.name).css('color', icon.color);
					$('.name', li).text(newName);
					$('li[data-path="' + newPath + '"]').attr('data-path', newPath);
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
	this.Copy = function (newPath) {
		if (!RoxyFilemanConf.COPYFILE) {
			alert(t('E_ActionDisabled'));
			return;
		}
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.COPYFILE), 'f', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newPath);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				f: this.fullPath,
				n: newPath
			},
			dataType: 'json',
			async: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					var d = Directory.Parse(newPath);
					if (d) {
						d.files++;
						d.Update();
						d.SetStatusBar();
						if (!$("#pnlDirList").data("indeterm")) {
							d.ListFiles(true);
						}
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
		if (!RoxyFilemanConf.MOVEFILE) {
			alert(t('E_ActionDisabled'));
			return;
		}
		newFullPath = RoxyUtils.MakePath(newPath, this.name);
		var url = RoxyUtils.AddParam(RoxyUtils.GetRootPath(RoxyFilemanConf.MOVEFILE), 'f', this.fullPath);
		url = RoxyUtils.AddParam(url, 'n', newFullPath);
		var item = this;
		var ret = false;
		$.ajax({
			url: url,
			type: 'POST',
			data: {
				f: this.fullPath,
				n: newFullPath
			},
			dataType: 'json',
			async: false,
			success: function (data) {
				if (data.res.toLowerCase() == 'ok') {
					$('li[data-path="' + item.fullPath + '"]').remove();
					var d = Directory.Parse(item.path);
					if (d) {
						d.files--;
						d.Update();
						d.SetStatusBar();
						d = Directory.Parse(newPath);
						d.files++;
						d.Update();
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
}

File.Parse = function (path) {
	var ret = false;
	var li = $('#pnlFileList').find('li[data-path="' + path + '"]');
	if (li.length > 0)
		ret = new File(li.data('path'), li.data('size'), li.data('time'), li.data('w'), li.data('h'));

	return ret;
};