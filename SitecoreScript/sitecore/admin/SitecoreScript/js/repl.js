// @ts-nocheck
/* global module:false, define:false */
(function (root, factory) {
    'use strict';
    if (typeof define === 'function' && define.amd) {
        define([
            'codemirror',
            'codemirror-addon-lint-fix',
            'codemirror-addon-infotip',
            'codemirror/addon/hint/show-hint',
            'codemirror/mode/clike/clike',
            'codemirror/mode/vb/vb'
        ], factory);
    }
    else if (typeof module === 'object' && module.exports) {
        module.exports = factory(
            require('codemirror'),
            require('codemirror-addon-lint-fix'),
            require('codemirror-addon-infotip'),
            require('codemirror/addon/hint/show-hint'),
            require('codemirror/mode/clike/clike'),
            require('codemirror/mode/vb/vb')
        );
    }
    else {
        root.mirrorsharpREPL = factory(root.CodeMirror);
    }
})(this, function (CodeMirror) { // eslint-disable-line no-unused-vars
    'use strict';

    /** @type {<T>(target: T, ...sources: Array<T>) => T} */
    // eslint-disable-next-line es/no-object-assign
    const assign = Object.assign || function (target) {
        for (var i = 1; i < arguments.length; i++) {
            var source = arguments[i];
            for (var key in source) {
                // @ts-ignore
                target[key] = source[key];
            }
        }
        return target;
    };

    /**
     * @this {internal.SelfDebug}
     */
    function SelfDebug() {
        /** @type {() => string} */
        var getText;
        /** @type {() => number} */
        var getCursorIndex;
        /** @type {Array<internal.SelfDebugLogEntryData>} */
        const clientLog = [];
        /** @type {Array<internal.SelfDebugLogEntryData>} */
        var clientLogSnapshot;

        /**
         * @param {() => string} getTextValue
         * @param {() => number} getCursorIndexValue
         */
        this.watchEditor = function (getTextValue, getCursorIndexValue) {
            getText = getTextValue;
            getCursorIndex = getCursorIndexValue;
        };

        /**
         * @param {string} event
         * @param {string} message
         */
        this.log = function (event, message) {
            clientLog.push({
                time: new Date(),
                event: event,
                message: message,
                text: getText(),
                cursor: getCursorIndex()
            });
            while (clientLog.length > 100) {
                clientLog.shift();
            }
        };

        /**
         * @param {internal.Connection} connection
         */
        this.requestData = function (connection) {
            clientLogSnapshot = clientLog.slice(0);
            connection.sendRequestSelfDebugData();
        };

        /**
         * @param {internal.SelfDebugMessage} serverData
         */
        this.displayData = function (serverData) {
            /** @type {Array<{ entry: internal.SelfDebugLogEntryData, on: string, index: number }>} */
            const log = [];
            // ReSharper disable once DuplicatingLocalDeclaration
            /* eslint-disable block-scoped-var */
            for (var i = 0; i < clientLogSnapshot.length; i++) {
                log.push({ entry: clientLogSnapshot[i], on: 'client', index: i });
            }
            // ReSharper disable once DuplicatingLocalDeclaration
            // eslint-disable-next-line no-redeclare
            for (var i = 0; i < serverData.log.length; i++) {
                log.push({ entry: serverData.log[i], on: 'server', index: i });
            }
            /* eslint-enable block-scoped-var */
            log.sort(function (a, b) {
                if (a.on !== b.on) {
                    if (a.entry.time > b.entry.time) return +1;
                    if (a.entry.time < b.entry.time) return -1;
                    return 0;
                }
                if (a.index > b.index) return +1;
                if (a.index < b.index) return -1;
                return 0;
            });

            console.table(log.map(function (l) { // eslint-disable-line no-console
                var time = l.entry.time;
                var displayTime = ('0' + time.getHours()).slice(-2) + ':' + ('0' + time.getMinutes()).slice(-2) + ':' + ('0' + time.getSeconds()).slice(-2) + '.' + time.getMilliseconds();
                return {
                    time: displayTime,
                    message: l.entry.message,
                    event: l.on + ':' + l.entry.event,
                    cursor: l.entry.cursor,
                    text: l.entry.text
                };
            }));
        };
    }

    /**
     * @this {internal.Connection}
     * @param {string} url
     * @param {internal.SelfDebug} selfDebug
     * */
    function Connection(url, selfDebug) {
        /** @type {WebSocket} */
        var socket;
        /** @type {Promise} */
        var openPromise;
        const handlers = {
            /** @type {Array<Function>} */
            open: [],
            /** @type {Array<Function>} */
            message: [],
            /** @type {Array<Function>} */
            error: [],
            /** @type {Array<Function>} */
            close: []
        };

        open();

        var mustBeClosed = false;
        var reopenPeriod = 0;
        /** @type {number} */
        var reopenPeriodResetTimer;
        var reopening = false;

        function open() {
            socket = new WebSocket(url);
            openPromise = new Promise(function (resolve) {
                socket.addEventListener('open', function () {
                    reopenPeriodResetTimer = setTimeout(function () { reopenPeriod = 0; }, reopenPeriod);
                    resolve();
                });
            });

            for (var key in handlers) {
                const keyFixed = key;
                // @ts-ignore
                const handlersByKey = handlers[key];
                socket.addEventListener(key, function (e) {
                    const handlerArguments = [e];
                    if (keyFixed === 'message') {
                        // @ts-ignore
                        const data = JSON.parse(e.data);
                        if (data.type === 'self:debug') {
                            for (var entry of data.log) {
                                entry.time = new Date(entry.time);
                            }
                        }
                        if (selfDebug)
                            selfDebug.log('before', JSON.stringify(data));
                        handlerArguments.unshift(data);
                    }
                    for (var handler of handlersByKey) {
                        handler.apply(null, handlerArguments);
                    }
                    if (selfDebug && keyFixed === 'message')
                        selfDebug.log('after', JSON.stringify(handlerArguments[0]));
                });
            }
        }

        function tryToReopen() {
            if (mustBeClosed || reopening)
                return;

            if (reopenPeriodResetTimer) {
                clearTimeout(reopenPeriodResetTimer);
                reopenPeriodResetTimer = null;
            }

            reopening = true;
            setTimeout(function () {
                open();
                reopening = false;
            }, reopenPeriod);
            if (reopenPeriod < 60000)
                reopenPeriod = Math.min(5 * (reopenPeriod + 200), 60000);
        }

        /**
         * @param {string} command
         */
        function sendWhenOpen(command) {
            if (mustBeClosed)
                throw "Cannot send command '" + command + "' after the close() call.";
            return openPromise.then(function () {
                if (selfDebug)
                    selfDebug.log('send', command);
                socket.send(command);
            });
        }

        /**
         * @param {string} key
         * @param {Function} handler
         */
        this.on = function (key, handler) {
            // @ts-ignore
            handlers[key].push(handler);
        };
        /**
        * @param {string} key
        * @param {Function} handler
        */
        this.off = function (key, handler) {
            // @ts-ignore
            const list = handlers[key];
            const index = list.indexOf(handler);
            if (index >= 0)
                list.splice(index, 1);
        };

        const removeEvents = addEvents(this, {
            error: tryToReopen,
            close: tryToReopen
        });

        /**
        * @param {number} start
        * @param {number} length
        * @param {string} newText
        * @param {number} cursorIndexAfter
        * @param {string} reason
        */
        this.sendReplaceText = function (start, length, newText, cursorIndexAfter, reason) {
            return sendWhenOpen('R' + start + ':' + length + ':' + cursorIndexAfter + ':' + (reason || '') + ':' + newText);
        };

        /**
        * @param {number} cursorIndex
        */
        this.sendMoveCursor = function (cursorIndex) {
            return sendWhenOpen('M' + cursorIndex);
        };

        /**
        * @param {string} char
        */
        this.sendTypeChar = function (char) {
            return sendWhenOpen('C' + char);
        };

        /** @type {{ ['cancel']: 'X'; ['force']: 'F'; [index: number]: undefined }} */
        const stateCommandMap = { cancel: 'X', force: 'F' };

        /**
        * @param {internal.StateCommand|'info'|number} indexOrCommand
        * @param {number} [indexIfInfo]
        */
        this.sendCompletionState = function (indexOrCommand, indexIfInfo) {
            const argument = indexOrCommand !== 'info'
                ? (stateCommandMap[indexOrCommand] || indexOrCommand)
                : 'I' + indexIfInfo;
            return sendWhenOpen('S' + argument);
        };

        /**
        * @param {internal.StateCommand} command
        */
        this.sendSignatureHelpState = function (command) {
            return sendWhenOpen('P' + stateCommandMap[command]);
        };

        /**
        * @param {number} cursorIndex
        */
        this.sendRequestInfoTip = function (cursorIndex) {
            return sendWhenOpen('I' + cursorIndex);
        };

        this.sendSlowUpdate = function () {
            return sendWhenOpen('U');
        };

        /**
        * @param {number} actionId
        */
        this.sendApplyDiagnosticAction = function (actionId) {
            return sendWhenOpen('F' + actionId);
        };

        /**
        * @param {object} options
        */
        this.sendSetOptions = function (options) {
            const optionPairs = [];
            for (var key in options) {
                optionPairs.push(key + '=' + options[key]);
            }
            return sendWhenOpen('O' + optionPairs.join(','));
        };

        this.sendRequestSelfDebugData = function () {
            return sendWhenOpen('Y');
        };

        this.close = function () {
            mustBeClosed = true;
            removeEvents();
            socket.close();
        };
    }

    /**
     * @this {internal.Hinter}
     * @param {CodeMirror.Editor} cm
     * @param {internal.Connection} connection
     * */
    function Hinter(cm, connection) {
        const indexInListKey = '$mirrorsharp-indexInList';
        const priorityKey = '$mirrorsharp-priority';
        const cachedInfoKey = '$mirrorsharp-cached-info';

        /** @type {'starting'|'started'|'stopped'|'committed'} */
        var state = 'stopped';
        /** @type {boolean} */
        var hasSuggestion;
        /** @type {internal.CompletionOptionalData} */
        var currentOptions;
        /** @type {{ item: internal.HintEx, index: number, element: HTMLElement }} */
        var selected;

        /** @type {number} */
        var infoLoadTimer;

        /** @type {internal.HintEx['hint']} */
        const commit = function (_, data, item) {
            connection.sendCompletionState(item[indexInListKey]);
            state = 'committed';
        };

        const cancel = function () {
            if (cm.state.completionActive)
                cm.state.completionActive.close();
        };

        const removeCMEvents = addEvents(cm, {
            /**
             * @param {any} _
             * @param {KeyboardEvent} e
             * */
            keypress: function (_, e) {
                if (state === 'stopped')
                    return;
                const key = e.key || String.fromCharCode(e.charCode || e.keyCode);
                if (currentOptions.commitChars.indexOf(key) > -1) {
                    const widget = cm.state.completionActive.widget;
                    if (!widget) {
                        cancel();
                        return;
                    }
                    widget.pick();
                }
            },

            endCompletion: function () {
                if (state === 'starting')
                    return;
                if (state === 'started')
                    connection.sendCompletionState('cancel');
                state = 'stopped';
                hideInfoTip();
                if (infoLoadTimer)
                    clearTimeout(infoLoadTimer);
            }
        });

        /**
         * @param {ReadonlyArray<internal.CompletionItemData>} list
         * @param {internal.SpanData} span
         * @param {internal.CompletionOptionalData} options
         */
        this.start = function (list, span, options) {
            state = 'starting';
            currentOptions = options;
            const hintStart = cm.posFromIndex(span.start);
            const hintList = list.map(function (c, index) {
                /** @type {internal.HintEx} */
                const item = {
                    text: c.filterText,
                    displayText: c.displayText,
                    // [TEMP] c.tags is for backward compatibility with 0.10
                    className: 'mirrorsharp-hint ' + kindsToClassName(c.kinds || c.tags),
                    hint: commit
                };
                item[indexInListKey] = index;
                item[priorityKey] = c.priority;
                if (c.span)
                    item.from = cm.posFromIndex(c.span.start);
                return item;
            });
            const suggestion = options.suggestion;
            hasSuggestion = !!suggestion;
            if (hasSuggestion) {
                hintList.unshift({
                    displayText: suggestion.displayText,
                    className: 'mirrorsharp-hint mirrorsharp-hint-suggestion',
                    hint: cancel
                });
            }
            cm.showHint({
                hint: function () { return getHints(hintList, hintStart); },
                completeSingle: false
            });
            state = 'started';
        };

        this.showTip = showInfoTip;

        this.destroy = function () {
            cancel();
            removeCMEvents();
        };

        /**
         * @param {ReadonlyArray<internal.HintEx>} list
         * @param {CodeMirror.Pos} start
         */
        function getHints(list, start) {
            const prefix = cm.getRange(start, cm.getCursor());
            if (prefix.length > 0) {
                var regexp = new RegExp('^' + prefix.replace(/[-/\\^$*+?.()|[\]{}]/g, '\\$&'), 'i');
                list = list.filter(function (item, index) {
                    return (hasSuggestion && index === 0) || regexp.test(item.text);
                });
                if (hasSuggestion && list.length === 1)
                    list = [];
            }
            if (!hasSuggestion) {
                // does not seem like I can use selectedHint here, as it does not force the scroll
                var selectedIndex = indexOfItemWithMaxPriority(list);
                if (selectedIndex > 0)
                    setTimeout(function () { cm.state.completionActive.widget.changeActive(selectedIndex); }, 0);
            }

            /** @type {internal.HintsResultEx} */
            const result = { from: start, list: list };
            CodeMirror.on(result, 'select', loadInfo);

            return result;
        }

        /**
         * @param {internal.HintEx} item
         * @param {HTMLElement} element
         */
        function loadInfo(item, element) {
            selected = { item: item, index: item[indexInListKey], element: element };
            if (infoLoadTimer)
                clearTimeout(infoLoadTimer);

            if (item[cachedInfoKey])
                showInfoTip(selected.index, item[cachedInfoKey]);

            infoLoadTimer = setTimeout(function () {
                connection.sendCompletionState('info', selected.index);
                clearTimeout(infoLoadTimer);
            }, 300);
        }

        /** @type {HTMLDivElement} */
        var infoTipElement;
        /** @type {number} */
        var currentInfoTipIndex;

        /**
         * @param {number} index
         * @param {ReadonlyArray<internal.PartData>} parts
         */
        function showInfoTip(index, parts) {
            // autocompletion disappeared while we were loading
            if (state !== 'started')
                return;

            // selected index changed while we were loading
            if (index !== selected.index)
                return;

            // we are already showing tooltip for this index
            if (index === currentInfoTipIndex)
                return;

            selected.item[cachedInfoKey] = parts;

            var element = infoTipElement;
            if (!element) {
                element = document.createElement('div');
                element.className = 'mirrorsharp-hint-info-tooltip mirrorsharp-any-tooltip mirrorsharp-theme';
                element.style.display = 'none';
                document.body.appendChild(element);
                infoTipElement = element;
            }
            else {
                while (element.firstChild) {
                    element.removeChild(element.firstChild);
                }
            }

            const top = selected.element.getBoundingClientRect().top;
            const left = selected.element.parentElement.getBoundingClientRect().right;
            const screenWidth = document.documentElement.getBoundingClientRect().width;

            const style = element.style;
            style.top = top + 'px';
            style.left = left + 'px';
            style.maxWidth = (screenWidth - left) + 'px';
            renderParts(infoTipElement, parts);
            style.display = 'block';

            currentInfoTipIndex = index;
        }

        function hideInfoTip() {
            currentInfoTipIndex = null;
            if (infoTipElement)
                infoTipElement.style.display = 'none';
        }

        /**
         * @param {ReadonlyArray<internal.HintEx>} list
         */
        function indexOfItemWithMaxPriority(list) {
            var maxPriority = 0;
            var result = 0;
            for (var i = 0; i < list.length; i++) {
                const priority = list[i][priorityKey];
                if (priority > maxPriority) {
                    result = i;
                    maxPriority = priority;
                }
            }
            return result;
        }
    }

    /**
     * @this {internal.SignatureTip}
     * @param {CodeMirror.Editor} cm
     */
    function SignatureTip(cm) {
        /** @type {{ [key: string]: string }} */
        const displayKindToClassMap = {
            keyword: 'cm-keyword'
        };

        var active = false;
        /** @type {HTMLDivElement} */
        var tooltip;
        /** @type {HTMLOListElement} */
        var ol;

        const hide = function () {
            if (!active)
                return;

            document.body.removeChild(tooltip);
            active = false;
        };

        /**
         * @param {ReadonlyArray<internal.SignatureData>} signatures
         * @param {internal.SpanData} span
         */
        this.update = function (signatures, span) {
            if (!tooltip) {
                tooltip = document.createElement('div');
                tooltip.className = 'mirrorsharp-theme mirrorsharp-any-tooltip mirrorsharp-signature-tooltip';
                ol = document.createElement('ol');
                tooltip.appendChild(ol);
            }

            if (!signatures || signatures.length === 0) {
                if (active)
                    hide();
                return;
            }

            while (ol.firstChild) {
                ol.removeChild(ol.firstChild);
            }
            for (var signature of signatures) {
                var li = document.createElement('li');
                if (signature.selected)
                    li.className = 'mirrorsharp-signature-selected';

                for (var part of signature.parts) {
                    var className = displayKindToClassMap[part.kind] || '';
                    if (part.selected)
                        className += ' mirrorsharp-signature-part-selected';

                    var child;
                    if (className) {
                        child = document.createElement('span');
                        child.className = className;
                        child.textContent = part.text;
                    }
                    else {
                        child = document.createTextNode(part.text);
                    }
                    li.appendChild(child);
                }
                ol.appendChild(li);
            }

            const startPos = cm.posFromIndex(span.start);

            active = true;

            const startCharCoords = cm.charCoords(startPos);
            tooltip.style.top = startCharCoords.bottom + 'px';
            tooltip.style.left = startCharCoords.left + 'px';
            document.body.appendChild(tooltip);
        };

        this.hide = hide;
    }

    const renderInfotip = (function () {
        /**
         * @param {HTMLElement} mainElement
         * @param {internal.InfotipSectionData} section
         * @param {number} index
         * @param {internal.InfotipMessage} info
         */
        function renderSection(mainElement, section, index, info) {
            const element = document.createElement('div');
            element.className = 'mirrorsharp-tip-' + section.kind;
            if (index === 0)
                element.className += ' ' + kindsToClassName(info.kinds);
            renderParts(element, section.parts);
            mainElement.appendChild(element);
        }

        return function (/** @type {HTMLElement} */ parent, /** @type {internal.InfotipMessage} */ data) {
            const wrapper = document.createElement('div');
            wrapper.className = 'mirrorsharp-theme mirrorsharp-tip-content';
            data.sections.forEach(function (section, index) {
                renderSection(wrapper, section, index, data);
            });
            parent.appendChild(wrapper);
        };
    })();

    /**
     * @param {HTMLTextAreaElement} textarea
     * @param {internal.Connection} connection
     * @param {internal.SelfDebug} selfDebug
     * @param {internal.EditorOptions} options
     * @this {public.Instance}
     */
    function Editor(textarea, connection, selfDebug, options) {
        const lineSeparator = '\r\n';
        /** @type {'C#'} */
        const defaultLanguage = 'C#';
        const languageModes = {
            'C#': 'text/x-csharp',
            'Visual Basic': 'text/x-vb',
            'F#': 'text/x-fsharp',
            'PHP': 'application/x-httpd-php'
        };

        /** @type {public.Language} */
        var language;
        /** @type {object} */
        var serverOptions;
        var lintingSuspended = true;
        var hadChangesSinceLastLinting = false;
        /** @type {CodeMirror.UpdateLintingCallback} */
        var capturedUpdateLinting;
        var replState = "initializing";
        var replErrorState = false;
        var submittedCode = [];

        options = assign({ language: defaultLanguage }, options);
        options.on = assign({
            slowUpdateWait: function () { },
            slowUpdateResult: function () { },
            textChange: function () { },
            connectionChange: function () { },
            /** @param {string} message */
            serverError: function (message) { throw new Error(message); }
        }, options.on);

        const cmOptions = assign({ gutters: [], indentUnit: 4 }, options.forCodeMirror, {
            lineSeparator: lineSeparator,
            mode: languageModes[options.language],
            lint: { async: true, getAnnotations: lintGetAnnotations, hasGutters: true },
            lintFix: { getFixes: getFixes }
        });
        if (!options.sharplabPreQuickInfoCompatibilityMode)
            cmOptions.infotip = { async: true, delay: 500, getInfo: infotipGetInfo, render: renderInfotip };

        cmOptions.gutters.push('CodeMirror-lint-markers');

        language = options.language;
        if (language !== defaultLanguage)
            serverOptions = { language: language };

        const cmSource = (function getCodeMirror() {
            /** @type {CodeMirror.Element} */
            // @ts-ignore
            const next = textarea.nextSibling;
            if (next && next.CodeMirror) {
                const existing = next.CodeMirror;
                for (var key in cmOptions) {
                    // @ts-ignore
                    existing.setOption(key, cmOptions[key]);
                }
                return { cm: existing, existing: true };
            }

            // @ts-ignore
            return { cm: CodeMirror.fromTextArea(textarea, cmOptions) };
        })();
        /** @type {CodeMirror.Editor} */
        // @ts-ignore
        const cm = cmSource.cm;

        const keyMap = {
            'Shift-Tab': 'indentLess',
            'Ctrl-Space': function () { connection.sendCompletionState('force'); },
            'Shift-Ctrl-Space': function () { connection.sendSignatureHelpState('force'); },
            'Ctrl-.': 'lintFixShow',
            'Shift-Ctrl-Y': selfDebug ? function () { selfDebug.requestData(connection); } : null
        };
        cm.addKeyMap(keyMap);
        // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
        // ensures that next 'id' will be -1 whether a change happened or not
        cm.state.lint.waitingFor = -2;
        if (!cmSource.existing)
            setText(textarea.value);

        /** @type {() => string} */
        const getText = cm.getValue.bind(cm);
        if (selfDebug)
            selfDebug.watchEditor(getText, getCursorIndex);

        const cmWrapper = cm.getWrapperElement();
        cmWrapper.classList.add('mirrorsharp', 'mirrorsharp-theme');

        const hinter = new Hinter(cm, connection);
        const signatureTip = new SignatureTip(cm);
        const removeConnectionEvents = addEvents(connection, {
            /** @param {Event} e */
            open: function (e) {
                hideConnectionLoss();
                if (serverOptions)
                    connection.sendSetOptions(serverOptions);

                const text = cm.getValue();
                if (text === '' || text == null) {
                    lintingSuspended = false;
                    return;
                }

                connection.sendReplaceText(0, 0, text, getCursorIndex());
                options.on.connectionChange('open', e);
                lintingSuspended = false;
                hadChangesSinceLastLinting = true;
                if (capturedUpdateLinting)
                    requestSlowUpdate();
            },
            message: onMessage,
            error: onCloseOrError,
            close: onCloseOrError
        });

        /** @param {CloseEvent|ErrorEvent} e */
        function onCloseOrError(e) {
            lintingSuspended = true;
            showConnectionLoss();
            options.on.connectionChange(e instanceof CloseEvent ? 'close' : 'error', e);
        }

        const indexKey = '$mirrorsharp-index';
        var changePending = false;
        /** @type {string} */
        var changeReason = null;
        var changesAreFromServer = false;
        const removeCMEvents = addEvents(cm, {
            /**
             * @param {CodeMirror.Editor} _
             * @param {CodeMirror.Change} change
             * */
            beforeChange: function (_, change) {
                change.from[indexKey] = cm.indexFromPos(change.from);
                change.to[indexKey] = cm.indexFromPos(change.to);
                changePending = true;
            },
            cursorActivity: function () {
                if (changePending)
                    return;
                connection.sendMoveCursor(getCursorIndex());
            },
            /**
            * @param {CodeMirror.Editor} _
            * @param {ReadonlyArray<CodeMirror.Change>} changes
            * */
            changes: function (_, changes) {
                hadChangesSinceLastLinting = true;
                changePending = false;
                const cursorIndex = getCursorIndex();
                changes = mergeChanges(changes);
                for (var i = 0; i < changes.length; i++) {
                    const change = changes[i];
                    const start = change.from[indexKey];
                    const length = change.to[indexKey] - start;
                    let text = change.text.join(lineSeparator);

                    //If error returned, handle server update
                    if (isReplResponseError()) removeFailingCode();
                    // ignore REPL response text on server side
                    if (isReplResponsePrintText()) text = text.replace(/./g, ' ').replace(' ', ";");

                    if (cursorIndex === start + 1 && text.length === 1 && !changesAreFromServer) {
                        if (length > 0)
                            connection.sendReplaceText(start, length, '', cursorIndex - 1);
                        connection.sendTypeChar(text);
                    }
                    else {
                        connection.sendReplaceText(start, length, text, cursorIndex, changeReason);
                    }                    
                }
                options.on.textChange(getText);
            }
        });
        /**
        * @param {ReadonlyArray<CodeMirror.Change>} changes
        * */
        function mergeChanges(changes) {
            if (changes.length < 2)
                return changes;
            const results = [];
            var before = null;
            for (const change of changes) {
                if (changesCanBeMerged(before, change)) {
                    before = {
                        // @ts-ignore (needs TS 3.1, https://github.com/Microsoft/TypeScript/pull/26343)
                        from: before.from,
                        // @ts-ignore
                        to: before.to,
                        // @ts-ignore
                        text: [before.text[0] + change.text[0]],
                        origin: change.origin
                    };
                }
                else {
                    if (before)
                        results.push(before);
                    before = change;
                }
            }
            results.push(before);
            return results;
        }

        /**
         * @param {CodeMirror.Change?} first
         * @param {CodeMirror.Change?} second
         * @return {boolean}
         */
        function changesCanBeMerged(first, second) {
            return first && second
                && first.origin === 'undo'
                && second.origin === 'undo'
                && first.to.line === second.from.line
                && first.text.length === 1
                && second.text.length === 1
                && second.from.ch === second.to.ch
                && (first.to.ch + first.text[0].length) === second.from.ch;
        }

        /** @param {internal.Message} message */
        function onMessage(message) {
            switch (message.type) {
                case 'changes':
                    receiveServerChanges(message.changes, message.reason);
                    break;

                case 'completions':
                    hinter.start(message.completions, message.span, {
                        commitChars: message.commitChars,
                        suggestion: message.suggestion
                    });
                    break;

                case 'completionInfo':
                    hinter.showTip(message.index, message.parts);
                    break;

                case 'signatures':
                    signatureTip.update(message.signatures, message.span);
                    break;

                case 'infotip':
                    if (!message.sections || isPrintLine(message.span.start)) {
                        cm.infotipUpdate(null);
                        return;
                    }
                    cm.infotipUpdate({
                        data: message,
                        range: spanToRange(message.span)
                    });
                    break;

                case 'slowUpdate':
                    showSlowUpdate(message);
                    break;

                case 'optionsEcho':
                    receiveServerOptions(message.options);
                    break;

                case 'self:debug':
                    selfDebug.displayData(message);
                    break;

                case 'error':
                    options.on.serverError(message.message);
                    break;

                default:
                    throw new Error('Unknown message type "' + message.type);
            }
        }

        function removeFailingCode() {
            var lastSubmmitedCode = submittedCode.pop();
            const cursorIndex = getCursorIndex();
            var start = cm.indexFromPos({ line: lastSubmmitedCode.fromLine, ch: 0 });
            var length = lastSubmmitedCode.code.length;
            var text = ' '.repeat(length);
            connection.sendReplaceText(start, length, text, cursorIndex);
        }

        /**
         * @param {string} _
         * @param {CodeMirror.UpdateLintingCallback} updateLinting
         */
        function lintGetAnnotations(_, updateLinting) {
            if (!capturedUpdateLinting) {
                capturedUpdateLinting = function () {
                    // see https://github.com/codemirror/CodeMirror/blob/dbaf6a94f1ae50d387fa77893cf6b886988c2147/addon/lint/lint.js#L133
                    // ensures that next 'id' will always match 'waitingFor'
                    cm.state.lint.waitingFor = -1;
                    // eslint-disable-next-line no-invalid-this
                    updateLinting.apply(this, arguments);
                };
            }
            requestSlowUpdate();
        }

        function saveSubmittedCode(codeObj) {
            submittedCode.push(codeObj);
        }

        function isPrintLine(indexPos) {
            var gutterMarker = getGutterMarkerGlobalPosition(indexPos);
            return gutterMarker && gutterMarker === options.prompt.print;
        }

        function getGutterMarkerGlobalPosition(startPosition) {
            var lineInfoPos = cm.posFromIndex(startPosition);
            return getGutterMarkerFromLine(lineInfoPos.line);
        }

        function getGutterMarkerFromLine(lineNumber) {
            var hoveredLineInfo = cm.lineInfo(lineNumber);
            var gutterMarker = hoveredLineInfo.gutterMarkers && hoveredLineInfo.gutterMarkers[options.prompt.gutterName].textContent;
            return gutterMarker;
        }

        function isReplResponsePrintText() {
            return replState === "started-printing";
        }

        function isReplResponseError() {
            return replErrorState === true;
        }

        function setReplState(state) {
            replState = state;
        }

        function setReplErrorState(errorState) {
            replErrorState = errorState;
        }

        /** @returns {number} */
        function getCursorIndex() {
            return cm.indexFromPos(cm.getCursor());
        }

        /** @param {string} text */
        function setText(text) {
            cm.setValue(text.replace(/(\r\n|\r|\n)/g, '\r\n'));
        }

        /**
         * @param {ReadonlyArray<internal.ChangeData>} changes
         * @param {string} reason
         */
        function receiveServerChanges(changes, reason) {
            changesAreFromServer = true;
            changeReason = reason || 'server';
            for (var change of changes) {
                const from = cm.posFromIndex(change.start);
                const to = change.length > 0 ? cm.posFromIndex(change.start + change.length) : from;
                cm.replaceRange(change.text, from, to, '+server');
            }
            changeReason = null;
            changesAreFromServer = false;
        }

        /**
         * @param {CodeMirror.Editor} cm
         * @param {number} line
         * @param {ReadonlyArray<CodeMirror.LintAnnotation>} annotations
         * @return {ReadonlyArray<CodeMirror.LintFix>}
         */
        // eslint-disable-next-line no-shadow
        function getFixes(cm, line, annotations) {
            /** @type {Array<CodeMirror.LintFix>} */
            var fixes = [];
            for (var i = 0; i < annotations.length; i++) {
                const diagnostic = annotations[i].diagnostic;
                if (!diagnostic.actions)
                    continue;
                for (var j = 0; j < diagnostic.actions.length; j++) {
                    var action = diagnostic.actions[j];
                    fixes.push({
                        text: action.title,
                        apply: requestApplyFixAction,
                        id: action.id
                    });
                }
            }
            return fixes;
        }

        /**
         * @param {CodeMirror.Editor} cm
         * @param {number} line
         * @param {CodeMirror.LintFix} fix
         */
        // eslint-disable-next-line no-shadow
        function requestApplyFixAction(cm, line, fix) {
            connection.sendApplyDiagnosticAction(fix.id);
        }

        /**
         * @param {CodeMirror.Editor} cm
         * @param {CodeMirror.Pos} position
         */
        // eslint-disable-next-line no-shadow
        function infotipGetInfo(cm, position) {
            connection.sendRequestInfoTip(cm.indexFromPos(position));
        }

        /** @param {boolean} [force] */
        function requestSlowUpdate(force) {
            if (lintingSuspended || !(hadChangesSinceLastLinting || force))
                return null;
            hadChangesSinceLastLinting = false;
            options.on.slowUpdateWait();
            return connection.sendSlowUpdate();
        }

        /** @param {internal.SlowUpdateMessage} update */
        function showSlowUpdate(update) {
            /** @type {Array<CodeMirror.LintAnnotation>} */
            const annotations = [];
            for (var diagnostic of update.diagnostics) {
                /** @type {public.DiagnosticSeverity|'unnecessary'} */
                var severity = diagnostic.severity;
                if (diagnostic.severity === 'hidden') {
                    if (diagnostic.tags.indexOf('unnecessary') < 0)
                        continue;

                    severity = 'unnecessary';
                }

                var range = spanToRange(diagnostic.span);
                annotations.push({
                    severity: severity,
                    message: diagnostic.message,
                    from: range.from,
                    to: range.to,
                    diagnostic: diagnostic
                });
            }
            capturedUpdateLinting(cm, annotations);
            options.on.slowUpdateResult({
                diagnostics: update.diagnostics,
                x: update.x
            });
        }

        /** @type {HTMLDivElement} */
        var connectionLossElement;
        function showConnectionLoss() {
            const wrapper = cm.getWrapperElement();
            if (!connectionLossElement) {
                connectionLossElement = document.createElement('div');
                connectionLossElement.setAttribute('class', 'mirrorsharp-connection-issue');
                connectionLossElement.innerText = 'Server connection lost, reconnecting…';
                wrapper.appendChild(connectionLossElement);
            }

            wrapper.classList.add('mirrorsharp-connection-has-issue');
        }

        function hideConnectionLoss() {
            cm.getWrapperElement().classList.remove('mirrorsharp-connection-has-issue');
        }

        /** @param {public.ServerOptions} value */
        function sendServerOptions(value) {
            return connection.sendSetOptions(value).then(function () {
                return requestSlowUpdate(true);
            });
        }

        /** @param {public.ServerOptions} value */
        function receiveServerOptions(value) {
            serverOptions = value;
            if (value.language !== undefined && value.language !== language) {
                language = value.language;
                cm.setOption('mode', languageModes[language]);
            }
        }

        /**
         * @param {public.SpanData} span
         * @returns {internal.Range}
         * */
        function spanToRange(span) {
            return {
                from: cm.posFromIndex(span.start),
                to: cm.posFromIndex(span.start + span.length)
            };
        }

        /** @param {public.DestroyOptions} destroyOptions */
        function destroy(destroyOptions) {
            cm.save();
            removeConnectionEvents();
            if (!destroyOptions.keepCodeMirror) {
                cm.toTextArea();
                return;
            }
            cm.removeKeyMap(keyMap);
            removeCMEvents();
            cm.setOption('lint', null);
            cm.setOption('lintFix', null);
            cm.setOption('infotip', null);
        }

        this.getCodeMirror = function () { return cm; };
        this.setText = setText;
        this.getLanguage = function () { return language; };
        /** @param {public.Language} value */
        this.setLanguage = function (value) { return sendServerOptions({ language: value }); };
        this.sendServerOptions = sendServerOptions;
        this.destroy = destroy;
        this.setReplState = setReplState;
        this.getGutterMarkerFromLine = getGutterMarkerFromLine;
        this.setReplErrorState = setReplErrorState;
        this.saveSubmittedCode = saveSubmittedCode;
    }

    /**
     * @param {internal.EventSource} target
     * @param {{ [key: string]: Function }} handlers
     * @returns {() => void}
     * */
    function addEvents(target, handlers) {
        for (var key in handlers) {
            target.on(key, handlers[key]);
        }
        return function () {
            // eslint-disable-next-line no-shadow
            for (var key in handlers) {
                target.off(key, handlers[key]);
            }
        };
    }

    /**
     * @param {ReadonlyArray<string>} kinds
     * @returns {string}
     * */
    function kindsToClassName(kinds) {
        return 'mirrorsharp-has-kind ' + kinds.map(function (kind) {
            return 'mirrorsharp-kind-' + kind;
        }).join(' ');
    }

    /*
        const partKindClassMap = {
            text: 'mirrorsharp-tip-part-text',
            class: 'cm-type',
            struct: 'cm-type'
        };
    */

    /**
     * @param {HTMLElement} parent
     * @param {internal.PartData} part
     * @returns {void}
     * */
    function renderPart(parent, part) {
        const span = document.createElement('span');
        span.className = 'cm-' + part.kind;
        span.textContent = part.text;
        parent.appendChild(span);
    }

    /**
     * @param {HTMLElement} parent
     * @param {ReadonlyArray<internal.PartData>} parts
     * @returns {void}
     * */
    function renderParts(parent, parts) {
        parts.forEach(function (part) { renderPart(parent, part); });
    }

    /**
     * @param {HTMLTextAreaElement} textarea
     * @param {internal.Options} options
     * @returns {public.Instance}
     */
    function mirrorsharpREPL(textarea, options) {

        options = options || {};

        options = assign({
            electricChars: false,
            indentWithTabs: false,
            smartIndent: true
        }, options);

        var replkeymap = {
            "Up": up,
            "Down": down,
            "Delete": del,
            "Ctrl-Z": undo,
            "Enter": enter,
            "Ctrl-A": select,
            "Ctrl-Delete": del,
            "Shift-Enter": enter,
            "Backspace": backspace,
            "Ctrl-Backspace": backspace,
            "Tab": tab,
            "Shift-Tab": shiftTab
        };

        options.prompt = options.prompt || {};

        function getReadyMarker(text) {
            var readyMarker = document.createElement("div");
            readyMarker.style.color = "#404040";
            if (!text) text = ">>";
            readyMarker.innerHTML = text;
            return readyMarker;
        }

        function getWaitMarker(text) {
            var waitMarket = document.createElement("div");
            waitMarket.style.color = "#505050";
            if (!text) text = "..";
            waitMarket.innerHTML = text;
            return waitMarket;
        }

        function getPrintMarker(text) {
            var printMarker = document.createElement("div");
            printMarker.style.color = "#454545";
            if (!text) text = "--";
            printMarker.innerHTML = text;
            return printMarker;
        }

        var history = [];
        var buffer = [];
        var repl = options.module || {};
        var user = true;
        var text = "";
        var line = 0;
        var ch = 0;
        var n = 0;
        var startBlock = 0;

        const selfDebug = options.selfDebugEnabled ? new SelfDebug() : null;
        const connection = new Connection(options.serviceUrl, selfDebug);
        const editor = new Editor(textarea, connection, selfDebug, options);

        var cm = editor.getCodeMirror();
        cm.setGutterMarker(line, options.prompt.gutterName, getReadyMarker(options.prompt.ready));
        cm.addKeyMap(replkeymap);
        const removeReplEvents = addEvents(cm, { change: change });

        function undo() { }

        function up() {
            switch (n--) {
                case 0:
                    n = 0;
                    return;
                case history.length:
                    text = cm.getLine(line).slice(ch);
            }

            cm.replaceRange(history[n], { line: line, ch: 0 }, { line: line, ch: cm.getLine(line).length });
        }

        function down() {
            switch (n++) {
                case history.length:
                    n--;
                    return;
                case history.length - 1:
                    cm.replaceRange(text, { line: line, ch: 0 }, { line: line, ch: cm.getLine(line).length });
                    return;
            }

            cm.replaceRange(history[n], { line: line, ch: ch }, { line: line, ch: cm.getLine(line).length });
        }

        function select() {
            var length = cm.getLine(line).slice(ch).length;
            cm.setSelection({ line: line, ch: 0 }, { line: line, ch: length });
        }

        function backspace() {
            //var selected = cm.somethingSelected();
            //var cursor = cm.getCursor(true);
            //var ln = cursor.line;
            //var c = cursor.ch;

            //if (ln === line && c >= ch + (selected ? 0 : 1)) {
            //    if (!selected) cm.setSelection({ line: ln, ch: c - 1 }, cursor);
            //    cm.replaceSelection("");
            //}
            if (!cm.somethingSelected()) {
                let cursorsPos = cm.listSelections().map((selection) => selection.anchor);
                let indentUnit = cm.options.indentUnit;
                let shouldDelChar = false;
                for (let cursorIndex in cursorsPos) {
                    let cursorPos = cursorsPos[cursorIndex];
                    let indentation = cm.getStateAfter(cursorPos.line).indented;
                    if (!(indentation !== 0 &&
                        cursorPos.ch <= indentation &&
                        cursorPos.ch % indentUnit === 0)) {
                        shouldDelChar = true;
                    }
                }
                if (!shouldDelChar) {
                    cm.execCommand('indentLess');
                } else {
                    cm.execCommand('delCharBefore');
                }
            } else {
                cm.execCommand('delCharBefore');
            }
        }

        function tab(cm) {
            if (cm.getMode().name === 'null') {
                cm.execCommand('insertTab');
            } else {
                if (cm.somethingSelected()) {
                    cm.execCommand('indentMore');
                } else {
                    cm.execCommand('insertSoftTab');
                }
            }
        }

        function shiftTab(cm) {
            cm.execCommand('indentLess');
        }

        function del() {
            var cursor = cm.getCursor(true);
            var ln = cursor.line;
            var c = cursor.ch;

            if (ln === line && c < cm.getLine(ln).length && c >= ch) {
                if (!cm.somethingSelected()) cm.setSelection({ line: ln, ch: c + 1 }, cursor);
                cm.replaceSelection("");
            }
        }

        function change(cm, changes) {
            var to = changes.to;
            var from = changes.from;
            var text = changes.text;
            var next = changes.next;
            var length = text.length;

            if (user) {
                if (from.line < line || from.ch < ch) {
                    var mark = editor.getGutterMarkerFromLine(from.line);
                    if (mark === options.prompt.print) {
                        cm.undo();
                        return;
                    } else {
                        cm.undo();
                        return;
                    }
                }
                else if (length-- > 1) {
                    cm.undo();

                    var ln = cm.getLine(line).slice(ch);
                    text[0] = ln.slice(0, from.ch) + text[0];

                    for (var i = 0; i < length; i++) {
                        cm.replaceRange(text[i], { line: line, ch: ch }, { line: line, ch: cm.getLine(line).length });
                        enter();
                    }

                    cm.replaceRange(text[length] + ln.slice(to.ch), { line: line, ch: ch }, { line: line, ch: cm.getLine(line).length });
                }
            }

            if (next) change(cm, next);
        }

        function print(message, className, isError) {
            editor.setReplState("started-printing");
            editor.setReplErrorState(isError);
            var mode = user;
            var ln = line;
            user = false;

            message = String(message);
            var text = cm.getLine(line);

            if (text) {
                cm.setGutterMarker(line, options.prompt.gutterName, getWaitMarker("--"));
                var cursor = cm.getCursor().ch;
            }

            var newLines = message.split("\r\n").length - 1;
            if (newLines === 0) {
                message = message + "\r\n";
                newLines++;
            }

            cm.replaceRange(message, { line: line, ch: ch }, { line: line, ch: cm.getLine(line).length });            
            line += newLines;
            if (className) cm.markText({ line: ln, ch: 0 }, { line: line, ch: message.length }, className);

            for (var lineMarks = ln; lineMarks < line - 1; lineMarks++) {
                cm.setGutterMarker(lineMarks, options.prompt.gutterName, getPrintMarker(options.prompt.print));
            }

            if (text) {
                cm.replaceRange(text, { line: line, ch: ch }, { line: line, ch: cm.getLine(line).length });
                cm.setGutterMarker(line, options.prompt.gutterName, getReadyMarker(options.prompt.ready));
                cm.setCursor({ line: line, ch: cursor });
            }

            setTimeout(function () {
                user = mode;
            }, 0);
            editor.setReplState("stopped-printing");
            editor.setReplErrorState(false);
        }

        function enter() {
            var text = cm.getLine(line);
            ch = cm.getCursor().ch;
            var firstPartInput = text.slice(0, ch);
            var restInput = text.slice(ch);
            var input = text.slice(ch);
            user = false;

            if (text) {
                ch = 0;
                //buffer.push(input);
                buffer.push(firstPartInput);
                //n = history.push(input);
                n = history.push(firstPartInput);

                cm.replaceRange(firstPartInput + "\r\n" + restInput, { line: line, ch: ch }, { line: line, ch: cm.getLine(line).length });
                line++;

                //var code = buffer.join('\n').replace(/\r/g, '\n');
                var code = buffer.join('\r\n');
                repl.isBalanced(code, balanceResolved, balanceRejected);
            }

            setTimeout(function () {
                user = true;
            }, 0);
        }

        function balanceResolved(balanced) {
            if (balanced) {
                editor.saveSubmittedCode({ code: repl.replCode, fromLine: startBlock, toLine: line - 1 });
                repl.eval(repl.replCode, function (response) {
                    var className;
                    if (response.Results) repl.print(response.Results, className, response.IsError);
                    buffer.length = 0;
                    cm.setGutterMarker(line, options.prompt.gutterName, getReadyMarker(options.prompt.ready));
                    startBlock = line;
                    cm.scrollIntoView({ line: line, ch: 0 });
                    cm.setCursor({ line: line, ch: 0 });
                });
            } else {
                cm.setGutterMarker(line, options.prompt.gutterName, getWaitMarker(options.prompt.wait));
                cm.scrollIntoView({ line: line, ch: 0 });
                cm.setCursor({ line: line, ch: 0 });
            }            
        }

        function getReplHistory() {
            return history;
        }

        function balanceRejected(error) {
            repl.print(error);
        }

        function getCurrentLine() {
            return cm.getLine(line);
        }

        function setMode(mode) {
            cm.setOption("mode", mode);
        }

        function setTheme(theme) {
            cm.setOption("theme", theme);
        }

        function isBalanced() {
            return true;
        }

        function replEval() { }

        function destroy() {
            removeReplEvents();
        }

        repl.print = repl.print || print;
        repl.setMode = repl.setMode || setMode;
        repl.setTheme = repl.setTheme || setTheme;
        repl.isBalanced = repl.isBalanced || isBalanced;
        repl.eval = repl.eval || replEval;
        repl.getCurrentLine = repl.getCurrentLine || getCurrentLine;
        repl.destroy = repl.destroy || destroy;

        /** @type {object} */
        const exports = {};
        for (var key in editor) {
            // @ts-ignore
            exports[key] = editor[key].bind(editor);
        }
        /** @param {public.DestroyOptions} destroyOptions */
        exports.destroy = function (destroyOptions) {
            editor.destroy(destroyOptions);
            repl.destroy(destroyOptions);
            connection.close();
        };
        return exports;
    } return mirrorsharpREPL; // eslint-disable-line no-undef
});