/*
* User Interface Plugin (for single file uploads)
*
* Based on:
* jQuery File Upload User Interface Plugin
* https://github.com/blueimp/jQuery-File-Upload
*
* Copyright 2010, Sebastian Tschan
* https://blueimp.net
*
* Licensed under the MIT license:
* http://www.opensource.org/licenses/MIT
*/

(function ($) {
    var parentWidget = ($.blueimpFP || $.blueimp).fileupload;
    $.widget('blueimpUI.fileupload', parentWidget, {

        options: {
            dataType: 'json',
            namespace: 'fileupload-single-ui',
            minFileSize: undefined,
            maxFileSize: undefined,
            acceptFileTypes: /.+$/i
        },


        _getProgressInfo: function (data) {
            return _.formatBitrate(data.bitrate) + ' | ' +
                //_.formatTime(
                //    (data.total - data.loaded) * 8 / data.bitrate
                //) + ' | ' +
                _.formatPercentage(
                    data.loaded / data.total
                ) + ' | ' +
                _.formatFileSize(data.loaded) + ' / ' +
                _.formatFileSize(data.total);
        },

        _transition: function (node) {
            var dfd = $.Deferred();
            if ($.support.transition && node.hasClass('fade')) {
                node.on(
                    $.support.transitionEnd,
                    function (e) {
                        // Make sure we don't respond to other transitions events
                        // in the container element, e.g. from button elements:
                        if (e.target === node[0]) {
                        	node.off($.support.transitionEnd);
                            dfd.resolveWith(node);
                        }
                    }
                ).toggleClass('show in');
            } else {
                node.toggleClass('show in');
                dfd.resolveWith(node);
            }
            return dfd;
        },

        _validate: function (file) {
            if (file.error) {
                return file.error;
            }

            if (!(this.options.acceptFileTypes.test(file.type) || this.options.acceptFileTypes.test(file.name))) {
                return 'acceptFileTypes';
            }
            if (this.options.maxFileSize && file.size > this.options.maxFileSize) {
                return 'maxFileSize';
            }
            if (typeof file.size === 'number' && file.size < this.options.minFileSize) {
                return 'minFileSize';
            }

            return null;
        },

        _initSpecialOptions: function () {
            parentWidget.prototype._initSpecialOptions.call(this);
            this.options.dropZone = this.element;
        },

        _initEventHandlers: function () {
            parentWidget.prototype._initEventHandlers.call(this);

            var ns = this.options.namespace,
            	pre = 'fileupload',
            	eventData = { fileupload: this },
            	self = this,
            	el = this.element;

            function toggleButtons(showCancel) {
                var elUpload = el.find('.fileinput-button'),
	            	elCancel = el.find('.cancel');
                if (showCancel) {
                    elUpload.hide();
                    elCancel.show();
                }
                else {
                    elUpload.show();
                    elCancel.hide();
                }
            }

            this.element
            	.on(pre + 'dragover.' + ns, function (e) {
            	    e.preventDefault();
            	})
            	.on(pre + 'drop.' + ns, function (e, data) {
            	    if (data.files) {
            	        data.files = data.files.slice(0, 1);
            	    }
            	    if (data.originalFiles) {
            	        data.originalFiles = data.originalFiles.slice(0, 1);
            	    }
            	})
            	.on(pre + 'add.' + ns, function (e, data) {
            	    el.data('data', data);
            	})
            	.on(pre + 'send.' + ns, function (e, data) {
            	    var err = self._validate(data.files[0]);
            	    if (err) {
            	        if (err == "acceptFileTypes") {
            	            alert("Wrong filetype: expected " + self.options.acceptFileTypes.source);
            	        }
            	        return false;
            	    }

            	    toggleButtons(true);
            	    el.find('.fileupload-progress').addClass("show in");

            	    if (data.dataType && data.dataType.substr(0, 6) === 'iframe') {
            	        // Iframe Transport does not support progress events.
            	        // In lack of an indeterminate progress bar, we set
            	        // the progress to 100%, showing the full animated bar:
            	        el.find('.progress-bar').addClass(!$.support.transition && 'progress-animated')
	                       .attr('aria-valuenow', 100)
	                       .css('width', '100%');
            	    }
            	})
            	.on(pre + 'fail.' + ns, function (e, data) {
            	    //console.log("fail");
            	})
			    .on(pre + 'always.' + ns, function (e, data) {
			        var elProgress = el.find('.fileupload-progress');
			        if (!elProgress.hasClass("show in")) {
			            return
			        }
			        self._transition(elProgress).done(
		                function () {
		                    toggleButtons(false);
		                    elProgress
			                	.find('.progress-bar').attr('aria-valuenow', 0)
			                  	.css("width", "0%");
		                    elProgress
			                	.find('.progress-extended').html('&nbsp;');
		                }
	                );
			    })
			    .on(pre + 'progressall.' + ns, function (e, data) {
			        var elProgress = el.find('.fileupload-progress'),
	            		progress = parseInt(data.loaded / data.total * 100, 10),
	                    extendedProgressNode = elProgress.find('.progress-extended');

			        if (extendedProgressNode.length) {
			            extendedProgressNode.html(self._getProgressInfo(data));
			        }

			        elProgress
	                    .find('.progress-bar')
	                    .attr('aria-valuenow', progress)
	                    .css('width', progress + '%');
			    })
            // cancel button
                .on('click.' + ns, 'button.cancel', eventData, function (e) {
                    e.preventDefault();
                    var data = el.data('data') || {};
                    data.errorThrown = 'abort';
                    if (data.jqXHR) {
                        data.jqXHR.abort();
                    }
                    else {
                        data.errorThrown = 'abort';
                        e.data.fileupload._trigger('fail', e, data);
                    }
                });
        },

        _destroyEventHandlers: function () {
            this.element.off('.' + this.options.namespace);
            parentWidget.prototype._destroyEventHandlers.call(this);
        },

        _enableFileInputButton: function () {
            this.element.find('.fileinput-button input')
                .prop('disabled', false)
                .parent().removeClass('disabled');
        },

        _disableFileInputButton: function () {
            this.element.find('.fileinput-button input')
                .prop('disabled', true)
                .parent().addClass('disabled');
        },

        /*_create: function () {
        parentWidget.prototype._create.call(this);
        },*/

        enable: function () {
            var wasDisabled = false;
            if (this.options.disabled) {
                wasDisabled = true;
            }
            parentWidget.prototype.enable.call(this);
            if (wasDisabled) {
                this.element.find('input, button').prop('disabled', false);
                this._enableFileInputButton();
            }
        },

        disable: function () {
            if (!this.options.disabled) {
                this.element.find('input, button').prop('disabled', true);
                this._disableFileInputButton();
            }
            parentWidget.prototype.disable.call(this);
        }

	});

	// Wrapper & global init
	$.fn.fileUploadWrapper = function (options) {
		return this.each(function () {
			var el = $(this),
				elRemove = el.find('.remove'),
				elCancel = el.find('.cancel')
				elFile = el.find('.fileinput-button'),
				accept = _.isString(el.data('accept')) ? new RegExp('(\.|\/)(' + el.data('accept') + ')$', 'i') : undefined;

			var opts = {
				url: el.data('upload-url'),
				dataType: 'json',
				acceptFileTypes: accept,
				pasteZone: null,
				send: function (e, data) {
					if (options.onUploading) options.onUploading.apply(this, [e, el, data]);
				},
				done: function (e, data) {
					var result = data.result;
					if (result.success) {

						if (el.data('show-remove-after-upload')) {
							elRemove.removeClass("hide");
						}

						var cnt = el.closest('.fileupload-container');
						cnt.find('.img-thumbnail').attr('src', data.result.imageUrl);
						cnt.find('.hidden').val(data.result.pictureId);

						elCancel.addClass("hide");
						elFile.removeClass("hide");

						if (options.onUploadCompleted) options.onUploadCompleted.apply(this, [e, el, data]);
					}
				},
				error: function (jqXHR, textStatus, errorThrown) {
					if (errorThrown === 'abort') {
						if (options.onAborted) options.onAborted.apply(this, el);
					}
					else {
						if (options.onError) options.onError.apply(this, [el, textStatus, errorThrown]);
					}
				},
				always: function (e, data) {
					if (options.onCompleted) options.onCompleted.apply(this, [e, el, data]);
				}
			};

			options = $.extend({}, opts, options)

			el.fileupload(options);

			elRemove.on('click', function (e) {
				e.preventDefault();

				var cnt = el.closest('.fileupload-container');
				cnt.find('.img-thumbnail').attr('src', el.data('fallback-url'));
				cnt.find('.hidden').val(0);
				$(this).addClass("hide");
				if (options.onFileRemove) options.onFileRemove.apply(this, [e, el]);
			});

			// TODO: work out better solution for external buttons
			$("#add-product-picture").on("click", function () {
				elRemove.trigger("click");
			});
		});
	};
})(jQuery);
