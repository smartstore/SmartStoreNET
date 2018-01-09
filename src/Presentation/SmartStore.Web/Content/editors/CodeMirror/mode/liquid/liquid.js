// Copyright (c) 2012 Henning Kiel
//
// Permission is hereby granted, free of charge, to any person obtaining a copy of
// this software and associated documentation files (the "Software"), to deal in
// the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do so,
// subject to the following conditions:

// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS
// FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR
// COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER
// IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN
// CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

CodeMirror.defineMode("liquid", function () {
	function chain(stream, state, f) {
		state.tokenize = f;
		return f(stream, state);
	}

	// Used as scratch variables to communicate multiple values without
	// consing up tons of objects.
	var type, content;
	function ret(tp, style, cont) {
		type = tp; content = cont;
		return style;
	}

	function liquidTokenBase(stream, state) {
		var ch = stream.next();
		if (/[\s]/.test(ch)) {
			return ret("whitespace", "liquid-whitespace");
		}
		else if (state.liquidMode == "output-markup" && ch == '}' && stream.eat('}')) {
			state.tokenize = nonLiquid;
			return ret("}}");
		}
		else if (state.liquidMode == "tag-markup" && ch == '%' && stream.eat('}')) {
			state.tokenize = nonLiquid;
			return ret("%}");
		}
		else if (ch == '"' || ch == "'")
			return chain(stream, state, liquidTokenString(ch));
		else if (/[\.,:\|\[\]]/.test(ch))
			return ret(ch);
		else if (/\d/.test(ch)) {
			stream.match(/^\d*(?:\.\d*)?/);
			return ret("number", "liquid-atom");
		}
		else {
			stream.eatWhile(/[\w_?<>=]/);
			var word = stream.current();
			return ret("word", "liquid-word", word);
		}

	}

	function liquidTokenString(quote) {
		return function (stream, state) {
			if (!nextUntilUnescaped(stream, quote))
				state.tokenize = liquidTokenBase;
			return ret("string", "liquid-string");
		};
	}

	function nextUntilUnescaped(stream, end) {
		var escaped = false, next;
		while ((next = stream.next()) != null) {
			if (next == end && !escaped)
				return false;
			// Liquid does not seem to have string quote escaping
			escaped = !escaped && next == "abc";
		}
		return escaped;
	}

	function nonLiquid(stream, state) {
		var ch = stream.next();
		state.liquidMode = null;
		if (ch == '{') {
			if (stream.eat('%')) {
				state.tokenize = liquidTokenBase;
				state.liquidMode = "tag-markup";
				return ret("{%", "liquid-markup-delimiter");
			}
			else if (stream.eat('{')) {
				state.tokenize = liquidTokenBase;
				state.liquidMode = "output-markup";
				return ret("{{", "liquid-markup-delimiter");
			}
		}
		stream.eatWhile(/[^{]/);
		return null;
	}


	function parseLiquid(state, type, content, stream) {
		var cc = state.cc;
		// Communicate our context to the combinators.
		// (Less wasteful than consing up a hundred closures on every call.)
		cx.state = state; cx.stream = stream; cx.marked = null, cx.cc = cc;

		while (true) {
			var combinator = cc.length ? cc.pop() : liquidMarkup;
			if (combinator(type, content)) {
				// Immediately work off any autocompletion objects on the stack
				
				while (cc.length && cc[cc.length - 1].ac) {
					cc.pop()();
				}				

				if (cx.marked) return cx.marked;
				return null;
			}
		}
	}

	// Combinator utils

	var cx = { state: null, column: null, marked: null, cc: null };
	function pass() {
		for (var i = arguments.length - 1; i >= 0; i--) cx.cc.push(arguments[i]);
	}
	function cont() {
		pass.apply(null, arguments);
		return true;
	}

	// Combinators

	function expect(wanted, marker, wantedValue) {
		return function expecting(type, value) {
			if (type == wanted && (typeof (value) == "undefined" || value == wantedValue)) {
				if (typeof (marker) == 'string') cx.marked = marker;
				return cont();
			}
			else return cont(arguments.callee);
		};
	}

	function markAc(type) {
		var result = function () {
			var state = cx.state;
			state.ac = type;
			pass(unmarkAc);
		};
		result.ac = true;

		return result;
	}

	function unmarkAc() {
		cx.state.ac = null;
	}

	function liquidMarkup(type) {
		if (type == "{{") return cont(markAc('variable'), expression, maybeFilters, expect("}}", "liquid-markup-delimiter"));
		if (type == "{%") return cont(markAc('tag'), tagName, expect("%}", "liquid-markup-delimiter"));
		return cont();
	}

	var endTag = /^end[\w]+$/;
	var zeroArgTags = { "live": true, "footer": true, "comment": true };

	function tagName(type, value) {
		if (type == "word") {
			if (endTag.test(value)) {
				cx.marked = "liquid-endtag-name";
			}
			else {
				cx.marked = "liquid-tag-name";
				if (value == "include") return cont(expression, maybeIncludeArgs);
				if (value == "cycle") return cont(maybeCycleName, commasep(expression, "%}", true));
				if (value == "if" || value == "unless") return cont(markAc('variable'), expression, maybeOperator);
				if (value == "capture") return cont(expression);
				if (value == "for") return cont(expression, expect("word", "liquid-keyword", "in"), markAc('variable'), expression, maybeForArgs);
				if (value == "assign") return cont(expression, expect('word', "liquid-operator", "="), markAc('variable'), expression);
				if (zeroArgTags.hasOwnProperty(value)) return cont();
			}
		}
		return cont();
	}

	function maybeForArgs(type, value) {
		if (type == 'word' && value == 'reversed') {
			cx.marked = "liquid-keyword";
			return cont(maybeTagAttributes);
		}
		return pass(maybeTagAttributes);
	}

	function maybeTagAttributes(type) {
		if (type == "word") {
			cx.marked = "liquid-tag-attribute-name";
			return cont(expect(":"), expression, maybeTagAttributes);
		}
		return pass();
	}

	function expression(type) {
		if (type == 'word') {
			cx.marked = "liquid-variable";
			return cont(maybeMethod);
		}
		return cont();
	}

	function maybeMethod(type) {
		if (type == '.') return cont(markAc('method'), method, maybeMethod);
		if (type == '[') return cont(expression, expect(']'), maybeMethod);
		return pass();
	}

	function method(type) {
		if (type == 'word') {
			cx.marked = "liquid-method";
		}
		return cont();
	}

	var operators = { "<": true, ">": true, "<=": true, ">=": true, "==": true, "contains": true, "and": true, "or": true };

	function maybeOperator(type, value) {
		if (operators.hasOwnProperty(value)) {
			cx.marked = "liquid-operator";
			return cont(markAc('variable'), expression, maybeOperator);
		}
		return pass();
	}

	function maybeCycleName() {
		// Liquid's cycle interprets its first argument as the cycle name if it is follow by a :.
		// In order to separate between a non-name variable argument and the name argument, we
		// do a look-ahead here.
		// If we find a : after our current position, the current expression is the cycle name.
		if (cx.stream.match(/\s*:/, false)) {
			cx.stream.match(/\s*/);
			cx.marked = "liquid-tag-cycle-name";
			return cont(expect(':'));
		}
		return pass();
	}

	function maybeIncludeArgs(type, value) {
		if (type == 'word') {
			if (value == 'for' || value == 'with') {
				cx.marked = "liquid-keyword";
				return cont(expression);
			}
		}
		return pass();
	}

	function maybeFilters(type) {
		if (type == '|') return cont(markAc('filter'), filterName, maybeFilterArgs, maybeFilters);
		return pass();
	}

	function filterName() {
		cx.marked = "liquid-filter";
		return cont();
	}

	function maybeFilterArgs(type) {
		if (type == ':') return cont(commasep(expression, { "}}": true, "|": true }, true));
		return pass(maybeFilters);
	}

	function commasep(what, end, doNotEat) {
		function checkForEnd(type) {
			return (typeof (end) == "string" && type == end) || (typeof (end) == "object" && end.hasOwnProperty(type));
		}
		function proceed(type) {
			if (type == ",") return cont(what, proceed);
			if (checkForEnd(type)) return doNotEat ? pass() : cont();
			return cont(expect(end));
		}
		return function commaSeparated(type) {
			if (checkForEnd(type)) return doNotEat ? pass() : cont();
			else return pass(what, proceed);
		};
	}

	return {
		startState: function () {
			return {
				tokenize: nonLiquid,
				cc: [],
				liquidMode: null,
				ac: null
			}
		},

		resetState: function (state) {
			state.tokenize = nonLiquid;
			state.cc = [];
			state.liquidMode = null;
			state.ac = null;
			return state;
		},

		token: function (stream, state) {
			type = null;

			// Liquid markup cannot span multiple lines
			if (stream.sol()) this.resetState(state);

			var style = state.tokenize(stream, state);

			// If we have found liquid markup, we always set some common CSS classes, so we can make use
			// of the style cascade.
			if (state.liquidMode != null) {
				style = (style || "") + " liquid liquid-" + state.liquidMode;
			}

			// No parsing necessary for white space inside liquid markup or when did not detect any liquid markup.
			if (type == "whitespace" || type == null) return style;

			var parseType = parseLiquid(state, type, content, stream);

			return (style || "") + " " + (parseType || "");
		}
	};
}, "htmlmixed");

CodeMirror.defineMIME("text/x-liquid", "liquid");
CodeMirror.defineMIME("application/x-liquid", "liquid");

CodeMirror.modeExtensions["liquid-html"] || CodeMirror.defineMode("liquid-html", function (c, p) {
	var htmlMode = CodeMirror.getMode(c, p.backdrop || "text/html");
	var liquidMode = CodeMirror.getMode(c, p.overlay || "text/x-liquid");
	var overlay = CodeMirror.overlayMode(htmlMode, liquidMode);

	overlay.innerMode = function (state) {
		return {
			state: state.base || state.htmlState,
			mode: (state.overlay && state.overlay.liquidMode) ? liquidMode : state.localMode || htmlMode
		};
	};

	return overlay;
});

// Compatibility with CodeMirror's formatting addon
if (CodeMirror.modeExtensions) {
	// If the current mode is 'htmlmixed', returns the extension of a mode located at
	// the specified position (can be htmlmixed, css or javascript). Otherwise, simply
	// returns the extension of the editor's current mode.
	CodeMirror.defineExtension("getModeExtAtPos", function (pos) {
		var token = this.getTokenAt(pos);
		if (token && token.state && token.state.mode)
			return CodeMirror.modeExtensions[token.state.mode == "html" ? "htmlmixed" : token.state.mode];
		else
			if (token && token.state && token.state.base && token.state.base.mode)
				return CodeMirror.modeExtensions[token.state.base.mode == "html" ? "htmlmixed" : token.state.base.mode];
			else
				return this.getModeExt();
	});

}

CodeMirror.defineExtension("commentRangeLiquid", function (isComment, from, to) {
	var curMode = this.getModeExtAtPos(this.getCursor());
	if (isComment) { // Comment range
		var commentedText = this.getRange(from, to);
		this.replaceRange("{% comment %}" + this.getRange(from, to) + "{% endcomment %}"
			, from, to);
		if (from.line == to.line && from.ch == to.ch) { // An empty comment inserted - put cursor inside
			this.setCursor(from.line, from.ch + "{% comment %}".length);
		}
	}
	else { // Uncomment range
		var selText = this.getRange(from, to);
		var startIndex = selText.indexOf("{% comment %}");
		var endIndex = selText.lastIndexOf("{% endcomment %}");
		if (startIndex > -1 && endIndex > -1 && endIndex > startIndex) {
			// Take string till comment start
			selText = selText.substr(0, startIndex)
				// From comment start till comment end
				+ selText.substring(startIndex + "{% comment %}".length, endIndex)
				// From comment end till string end
				+ selText.substr(endIndex + "{% endcomment %}".length);
		}
		this.replaceRange(selText, from, to);
	}
});


(function () {
	var Pos = CodeMirror.Pos;

	var liquidTags = ("assign block break capture case comment continue cycle elsif else " +
		"extends for if include literal raw tablerow unless zone").split(" ");

	var liquidFilters = ("Abs Append AtLeast AtMost Capitalize Ceil Compact Currency Date Default " +
		"DividedBy Downcase Escape First Floor FormatAddress H Handleize ImgTag Join Json Last Lstrip " +
		"Map Md5 Minus Modulo NewlineToBr Pluralize Plus Prepend Prettify Remove RemoveFirst Replace " +
		"ReplaceFirst Reverse Round Rstrip SanitizeHtmlId ScriptTag Sha1 Size Slice Sort " +
		"Split Strip StripHtml StripNewlines StylesheetTag T Times Truncate TruncateWords Uniq Upcase UrlEncode UrlDecode").split(" ");

	function findTreeNode(node, str) {
		if (node && node.Children) {
			return _.find(node.Children, function (x) { return x.Value.Name == str; });
		}
		return null;
	}

	function findIndex(node, str) {
		var idx = 0;
		if (str && node && node.Children) {
			$.each(node.Children, function (i, x) {
				if (x.text == str) {
					idx = i;
					return false;
				}
			});
		}
		return idx;
	}

	function isType(token, s) {
		s = "liquid" + (s ? "-" : "") + (s || "");
		return (token.type || "").indexOf(s) >= 0;
	}

	function getCompletions(token, context, options) {
		var found = [],
			strings = [],
			start = token.string,
			root = options.templateName ? SmartStore.Admin.modelTrees[options.templateName] : null;

		function maybeAdd(str, kind) {
			var st = (start == "|" || start == "{%") ? "" : start;
			if (str.lastIndexOf(st, 0) == 0 && !_.contains(strings, str)) {
				strings.push(str);
				var className;
				if (_.isNumber(kind) || _.isString(kind)) {
					className = "cm-hint-kind-" + kind;
				}
				found.push({ text: str, className: className });
			}		
		}

		function gatherCompletions(obj) {
			if (obj === "tags") {
				_.each(liquidTags, function (x, i) { maybeAdd(x, "tag"); });
			}
			else if (obj === "filters") {
				_.each(liquidFilters, function (x, i) { maybeAdd(x, "filter"); });
			}
			else if (obj && obj.Children) {
				var sorted = _.sortBy(_.map(obj.Children, function (x) {
					return { text: x.Value.Name, kind: x.Value.Kind };
				}), "text");
				_.each(sorted, function (x, i) { maybeAdd(x.text, x.kind); });
			}
		}

		if (context && context.length) {
			// If this is a property, see if it belongs to some node we can
			// find in the current environment.
			var obj = context.pop(), base;

			if (isType(obj, "variable")) {
				base = findTreeNode(root, obj.string);
			}

			while (base != null && context.length) {
				base = findTreeNode(base, context.pop().string);
			}

			if (base != null) {
				gatherCompletions(base);
			}
		}
		else {
			if (isType(token, "filter")) {
				gatherCompletions("filters");
			}
			else if (isType(token, "tag-name")) {
				gatherCompletions("tags");
			}
			else if (isType(token, "output-markup")) {
				if (token.string == "|") {
					gatherCompletions("filters");
				}
			}
			else {
				gatherCompletions(root);
			}
		}

		return found;
	};

	CodeMirror.registerHelper("hint", "liquid", function (cm, options) {
		var cur = cm.getCursor(),
			token = cm.getTokenAt(cur);

		if (!isType(token)) {
			return null;
		}

		// If it's not a 'word-style' token, ignore the token.
		if (!/^[\w$_]*$/.test(token.string)) {
			//var t2 = seekDelim("|") || seekDelim("{%");
			t2 = CodeMirror.hint.seekLeftLiquidDelim(cm, cur, token);
			if (t2 != null && t2.string !== "{{") {
				token = t2;
			}
			else {
				token = {
					start: cur.ch, end: cur.ch, string: "", state: token.state,
					type: token.string == "." ? token.type + " liquid-method" : null
				};
			}
		}
		else if (token.end > cur.ch) {
			token.end = cur.ch;
			token.string = token.string.slice(0, cur.ch - token.start);
		}

		var tprop = token;
		// If it is a method, find out what it is part of.
		while (isType(tprop, "method")) {
			tprop = cm.getTokenAt(Pos(cur.line, tprop.start));
			if (tprop.string != ".") return;
			tprop = cm.getTokenAt(Pos(cur.line, tprop.start));
			if (!context) var context = [];
			context.push(tprop);
		}

		return {
			list: getCompletions(token, context, options),
			from: Pos(cur.line, token.start),
			to: Pos(cur.line, token.end)
		};
	});

	function completeAfter(cm, predicate) {
		var cur = cm.getCursor();
		if (!predicate || predicate()) setTimeout(function () {
			if (!cm.state.completionActive)
				cm.showHint({ completeSingle: false });
		}, 100);
		return CodeMirror.Pass;
	}

	CodeMirror.registerHelper("hint", "completeAfter", completeAfter);

	CodeMirror.registerHelper("hint", "completeIfAfterSpace", function (cm) {
		return completeAfter(cm, function () {
			var cur = cm.getCursor();
			var token = cm.getTokenAt(cur);

			if (CodeMirror.hint.isInLiquid(cm, cur, token)) {
				return CodeMirror.hint.seekLeftLiquidDelim(cm, cur, token);
			}

			return CodeMirror.hint.isInHtmlTag(cm, cur, token);
		});
	});

	CodeMirror.registerHelper("hint", "completeIfAfterLt", function (cm) {
		return completeAfter(cm, function () {
			var cur = cm.getCursor();
			return cm.getRange(Pos(cur.line, cur.ch - 1), cur) == "<";
		});
	});

	CodeMirror.registerHelper("hint", "completeIfInTag", function (cm) {
		return completeAfter(cm, function () {
			return CodeMirror.hint.isInHtmlTag(cm);
		});
	});

	CodeMirror.registerHelper("hint", "isInHtmlTag", function (cm, cur, token) {
		cur = cur || cm.getCursor();
		token = token || cm.getTokenAt(cur);
		if (token.type == "string" && (!/['"]/.test(token.string.charAt(token.string.length - 1)) || token.string.length == 1)) return false;
		var inner = CodeMirror.innerMode(cm.getMode(), token.state).state;
		return inner.tagName;
	});

	CodeMirror.registerHelper("hint", "isInLiquid", function (cm, cur, token) {
		cur = cur || cm.getCursor();
		token = token || cm.getTokenAt(cur);
		var inner = CodeMirror.innerMode(cm.getMode(), token.state);
		return inner && inner.mode && inner.mode.name == "liquid";
	});

	CodeMirror.registerHelper("hint", "seekLeftLiquidDelim", function (cm, cur, token) {
		cur = cur || cm.getCursor();
		token = token || cm.getTokenAt(cur);
		var pos = cur.ch - 1;
		var delims = ["{{", "{%", "|"];

		var t = token;
		while (t.string === " " || _.contains(delims, (t.string))) {
			t = cm.getTokenAt({ line: cur.line, ch: pos, sticky: cur.sticky });
			delim = t.string;
			pos--;
			if (delim !== " ") {
				if (delim === "|") t.type += " liquid-filter";
				else if (delim === "{%") t.type += " liquid-tag-name";
				else if (delim === "{{") t.type += " liquid-variable";
				t.start = cur.ch;
				t.end = cur.ch;
				return t;
			}
		}

		return null;
	});
})();

