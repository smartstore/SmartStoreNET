/* https://github.com/tylerecouture/summernote-lists  */

(function (factory) {
	/* global define */
	if (typeof define === "function" && define.amd) {
		// AMD. Register as an anonymous module.
		define(["jquery"], factory);
	} else if (typeof module === "object" && module.exports) {
		// Node/CommonJS
		module.exports = factory(require("jquery"));
	} else {
		// Browser globals
		factory(window.jQuery);
	}
})(function ($) {
	$.extend(true, $.summernote.lang, {
		"en-US": {
			tableStyles: {
				tooltip: "Table style",
				stylesExclusive: ["Basic", "Bordered"],
				stylesInclusive: ["Striped", "Condensed", "Hoverable"]
			}
		}
	});
	$.extend($.summernote.options, {
		tableStyles: {
			// Must keep the same order as in lang.tableStyles.styles*
			stylesExclusive: ["", "table-bordered"],
			stylesInclusive: ["table-striped", "table-sm", "table-hover"]
		}
	});

	// Extends plugins for emoji plugin.
	$.extend($.summernote.plugins, {
		tableStyles: function (context) {
			var self = this,
				ui = $.summernote.ui,
				options = context.options,
				lang = options.langInfo,
				$editor = context.layoutInfo.editor,
				$editable = context.layoutInfo.editable,
				editable = $editable[0];

			context.memo("button.tableStyles", function () {
				var button = ui.buttonGroup([
					ui.button({
						className: "dropdown-toggle",
						contents: ui.dropdownButtonContents(ui.icon(options.icons.magic), options),
						callback: function (btn) {
							btn.data("placement", "bottom");
							btn.attr("title", lang.tableStyles.tooltip);
							btn.tooltip();

							btn.on('click', function () {
								self.updateTableMenuState(btn);
							});
						},
						data: {
							toggle: "dropdown"
						}
					}),
					ui.dropdownCheck({
						className: "dropdown-table-style",
						checkClassName: options.icons.menuCheck,
						items: self.generateListItems(
							options.tableStyles.stylesExclusive,
							lang.tableStyles.stylesExclusive,
							options.tableStyles.stylesInclusive,
							lang.tableStyles.stylesInclusive
						),
						callback: function ($dropdown) {
							$dropdown.find("a").each(function () {
								$(this).click(function () {
									self.updateTableStyles(this);
								});
							});
						}
					})
				]);
				return button.render();
			});

			self.updateTableStyles = function (chosenItem) {
				const rng = context.invoke("createRange", $editable);
				const dom = $.summernote.dom;
				if (rng.isCollapsed() && rng.isOnCell()) {
					context.invoke("beforeCommand");
					var table = dom.ancestor(rng.commonAncestor(), dom.isTable);
					self.updateStyles(
						$(table),
						chosenItem,
						options.tableStyles.stylesExclusive
					);
				}
			};

			/* Makes sure the check marks are on the currently applied styles */
			self.updateTableMenuState = function ($dropdownButton) {
				const rng = context.invoke("createRange", $editable);
				const dom = $.summernote.dom;
				if (rng.isCollapsed() && rng.isOnCell()) {
					var $table = $(dom.ancestor(rng.commonAncestor(), dom.isTable));
					var $listItems = $dropdownButton.next().find("a");
					self.updateMenuState(
						$table,
						$listItems,
						options.tableStyles.stylesExclusive
					);
				}
			};

			/* The following functions might be turnkey in other menu lists
				  with exclusive and inclusive items that toggle CSS classes. */

			self.updateMenuState = function ($node, $listItems, exclusiveStyles) {
				var hasAnExclusiveStyle = false;
				$listItems.each(function () {
					var cssClass = $(this).data("value");
					if ($node.hasClass(cssClass)) {
						$(this).addClass("checked");
						if ($.inArray(cssClass, exclusiveStyles) != -1) {
							hasAnExclusiveStyle = true;
						}
					} else {
						$(this).removeClass("checked");
					}
				});

				// if none of the exclusive styles are checked, then check a blank
				if (!hasAnExclusiveStyle) {
					$listItems.filter('[data-value=""]').addClass("checked");
				}
			};

			self.updateStyles = function ($node, chosenItem, exclusiveStyles) {
				var cssClass = $(chosenItem).data("value");
				context.invoke("beforeCommand");
				// Exclusive class: only one can be applied at a time
				if ($.inArray(cssClass, exclusiveStyles) != -1) {
					$node.removeClass(exclusiveStyles.join(" "));
					$node.addClass(cssClass);
				} else {
					// Inclusive classes: multiple are ok
					$node.toggleClass(cssClass);
				}
				context.invoke("afterCommand");
			};

			self.generateListItems = function (
				exclusiveStyles,
				exclusiveLabels,
				inclusiveStyles,
				inclusiveLabels
			) {
				var index = 0;
				var list = "";

				for (const style of exclusiveStyles) {
					list += self.getListItem(style, exclusiveLabels[index], true);
					index++;
				}
				list += '<div class="dropdown-divider"></div>';
				index = 0;
				for (const style of inclusiveStyles) {
					list += self.getListItem(style, inclusiveLabels[index], false);
					index++;
				}
				return list;
			};

			self.getListItem = function (value, label, isExclusive) {
				var item =
					'<a href="javascript:void(0)" class="dropdown-item ' +
					(isExclusive ? "exclusive-item" : "inclusive-item") +
					'" ' +
					' data-value="' +
					value +
					'">' +
					'<i class="note-icon-menu-check"></i> ' +
					" " +
					label +
					"</a>";
				return item;
			};
		}
	});
});