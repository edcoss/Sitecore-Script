var SSE = SSE || {};

SSE.SelectedNodeId = "";
SSE.SelectedNodeType = "";
SSE.SelectedNodePath = "";
SSE.msEditor = {};
SSE.msRepl = {};
SSE.replCode = null;

SSE.ExpandCode = function () {
    var $element = $(this);
    var parent = $element.parent();
    var expandedCode = parent.children('.expanded-code');
    var isDisabled = $element.attr('disabled');
    if (isDisabled) {
        expandedCode.toggle();
        return false;
    }
    var evalCodeContainer = [];
    evalCodeContainer = SSE.EvalCodeCollector($element, evalCodeContainer);
    var indentValue = $element.attr('data-indent').toString();
    var iconPath = $element.attr('data-icon').toString();
    var fullCode = document.getElementById('code').value;

    var codeData = { source: fullCode, evalCode: evalCodeContainer, indent: indentValue };
    var dataToSend = JSON.stringify(codeData);
    //console.log(dataToSend);
    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        url: "ScriptEditor.aspx/EvaluateObject",
        data: JSON.stringify(codeData),
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("ERROR on EvaluateObject call.\n" +
                "XMLHttpRequest: " + XMLHttpRequest.responseText + "\n" +
                "Status: " + textStatus + "\n" +
                "Error thrown: " + errorThrown + "\n");
            console.dir(XMLHttpRequest);
        },
        success: function (result) {
            var htmlResult = $(result.d.Results);
            expandedCode.append(htmlResult);
            $('.executionTime').text('Executed in ' + result.d.ElapsedMilliseconds + ' ms');
            expandedCode.on('click', 'a.expand-code', SSE.ExpandCode);
            $element.attr('disabled', 'disabled');
            var linkImg = $element.children('img');
            linkImg.attr('src', iconPath);
        }
    });
    return false;
};

SSE.EvalCodeCollector = function (element, stack) {
    var parent = element.parent();
    var grandParent = parent.parent();
    var greatGrandParent = grandParent.parent();
    if (grandParent.attr('id') === 'outputContainer') {
        stack.unshift(element.attr('data-code').toString());
        return stack;
    } else {
        var anchor = parent.children('a.expand-code');
        stack.unshift(anchor.attr('data-code').toString());
        var nextAnchor = greatGrandParent.children('a.expand-code');
        stack = SSE.EvalCodeCollector(nextAnchor, stack);
        return stack;
    }
};

SSE.ShowScriptTreeModal = function (parameters) {
    var scriptTree = $(parameters.data.treeSelector);
    scriptTree.jstree('destroy');
    scriptTree.on("changed.jstree", function (e, data) {
        if (data.selected.length) {
            //console.dir(data);
            SSE.SelectedNodeId = data.selected[0];
            SSE.SelectedNodeType = data.node.data.type;
            SSE.SelectedNodePath = data.node.data.path;
        } else {
            SSE.SelectedNodeId = "";
        }
    });
    var scriptRootFolderId = scriptTree.data('rootid');
    var paramsData = { rootItemId: scriptRootFolderId };
    scriptTree.jstree({
        'core': {
            'multiple': false,
            'data': {
                'type': 'POST',
                'url': "ScriptEditor.aspx/LoadItems",
                'contentType': 'application/json',
                'data': JSON.stringify(paramsData)
            },
            'error': function (errorObj) {
                console.log('Error when loading items in tree.');
                console.dir(errorObj);
            }
        }
    });
    $(parameters.data.modalSelector).modal({
        keyboard: true,
        focus: true,
        show: true
    });
    return false;
};

SSE.GetScriptCode = function () {
    if (!SSE.SelectedNodeId) return false;
    var paramData = { itemId: SSE.SelectedNodeId };
    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        url: "ScriptEditor.aspx/GetScriptCode",
        data: JSON.stringify(paramData),
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("ERROR on GetScriptCode call.\n" +
                "XMLHttpRequest: " + XMLHttpRequest.responseText + "\n" +
                "Status: " + textStatus + "\n" +
                "Error thrown: " + errorThrown + "\n");
            console.dir(XMLHttpRequest);
        },
        success: function (result) {
            var code = result.d;
            if (code) {
                var codeElement = document.getElementById('code');
                codeElement.value = code;
                SSE.msEditor.setText(code);
                $('#loadScriptTreeModal').modal('hide');
                $('#scriptPath').text(SSE.SelectedNodePath);
                $('#scriptPathField').val(SSE.SelectedNodePath);
            }
        }
    });
    return false;
};

SSE.SaveScriptCode = function () {
    if (!SSE.SelectedNodeId) return false;
    if (!SSE.SelectedNodeType) return false;
    var scriptName = $('#scriptNameField').val();
    var fullCode = SSE.msEditor.getCodeMirror().getValue();
    var paramData = { itemId: SSE.SelectedNodeId, name: scriptName, code: fullCode };
    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        url: "ScriptEditor.aspx/SaveScriptCode",
        data: JSON.stringify(paramData),
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("ERROR on GetScriptCode call.\n" +
                "XMLHttpRequest: " + XMLHttpRequest.responseText + "\n" +
                "Status: " + textStatus + "\n" +
                "Error thrown: " + errorThrown + "\n");
            console.dir(XMLHttpRequest);
        },
        success: function (result) {
            $('#saveScriptTreeModal').modal('hide');
            var item = result.d;
            SSE.SelectedNodePath = item.data.path;
            $('#scriptPath').text(SSE.SelectedNodePath);
            $('#scriptPathField').val(SSE.SelectedNodePath);
        }
    });
};

SSE.isBalanced = function (code, resolve, reject) {
    SSE.replCode = code;
    var codeData = { source: code };
    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        url: "ScriptEditor.aspx/IsBalancedCode",
        data: JSON.stringify(codeData),
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("ERROR on EvaluateCode call.\n" +
                "XMLHttpRequest: " + XMLHttpRequest.responseText + "\n" +
                "Status: " + textStatus + "\n" +
                "Error thrown: " + errorThrown + "\n");
            console.dir(XMLHttpRequest);
            reject(errorThrown);
        },
        success: function (result) {
            resolve(result.d.IsBalanced);
        }
    });
    return false;
};
SSE.eval = function (code, resolve, reject) {
    var codeData = { source: code };
    $.ajax({
        type: 'POST',
        contentType: 'application/json',
        url: "ScriptEditor.aspx/EvaluateCode",
        data: JSON.stringify(codeData),
        error: function (XMLHttpRequest, textStatus, errorThrown) {
            console.log("ERROR on EvaluateCode call.\n" +
                "XMLHttpRequest: " + XMLHttpRequest.responseText + "\n" +
                "Status: " + textStatus + "\n" +
                "Error thrown: " + errorThrown + "\n");
            console.dir(XMLHttpRequest);
            reject(errorThrown);
        },
        success: function (result) {
            resolve(result.d);
        }
    });
    return false;
};

$(document).ready(function () {
    $('a.expand-code').on('click', SSE.ExpandCode);

    $('#scriptMode').on('click', function () {
        var $this = $(this);
        $this.parent().addClass('active');
        $('#replMode').removeClass('active');
        $('.script-mode-options').show();
        $('.script-editor').show();
        $('.script-repl').hide();
        $('.Left').css('width', '50rem');
        $('.Right').show();
    });

    $('#replMode').on('click', function () {
        var $this = $(this);
        $this.parent().addClass('active');
        $('#scriptMode').removeClass('active');
        $('.script-mode-options').hide();
        $('.script-repl').show();
        $('.script-editor').hide();    
        $('.Left').css('width','100%');
        $('.Right').hide();
    });

    $('#loadScript').on('click', {
        modalSelector: '#loadScriptTreeModal',
        treeSelector: '.load-script-tree'
    }, SSE.ShowScriptTreeModal);

    $('#saveScript').on('click', {
        modalSelector: '#saveScriptTreeModal',
        treeSelector: '.save-script-tree'
    }, SSE.ShowScriptTreeModal);

    $('.load-script-button').on('click', SSE.GetScriptCode);
    $('.save-script-button').on('click', SSE.SaveScriptCode);

    var scriptPath = $('#scriptPathField').val();
    if (scriptPath) {
        SSE.SelectedNodePath = scriptPath;
        $('#scriptPath').text(SSE.SelectedNodePath);
    }

    $('.script-repl').hide();

    'use strict';
    const editorTextarea = document.getElementById('code');
    const replTextarea = document.getElementById('repl');
    const outputResults = document.getElementById('Output');

    outputResults.style.height = (window.innerHeight - 100) + 'px';
    outputResults.style.display = "block";

    editorTextarea.style.height = (window.innerHeight - 100) + 'px';
    editorTextarea.style.display = "block";

    replTextarea.style.height = (window.innerHeight - 100) + 'px';
    replTextarea.style.display = "none";

    SSE.msEditor = mirrorsharp(editorTextarea, {
        serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/mirrorsharp'),
        selfDebugEnabled: true,
        language: 'C#',
        forCodeMirror: {
            theme: "default", // for more examples, look at: https://codemirror.net/demo/theme.html
            lineNumbers: true,
            autoCloseBrackets: false,
            matchBrackets: true,
            // To highlight on scrollbars as well, pass annotateScrollbar in options as below.
            highlightSelectionMatches: { showToken: /\w/, annotateScrollbar: true },
            scrollbarStyle: "simple" // or "overlay"
        },
        keymap: {
            'Ctrl-Enter': function () { document.getElementById('Roslyn').click(); }
        }
    });

    SSE.msRepl = mirrorsharpREPL(replTextarea, {
        module: SSE,
        serviceUrl: window.location.href.replace(/^http(s?:\/\/[^/]+).*$/i, 'ws$1/msrepl'),
        selfDebugEnabled: true,
        language: 'C#',
        prompt: {
            gutterName: 'repl-prompt',
            ready: ">>",
            wait: "..",
            print: "--"
        },
        forCodeMirror: {
            theme: "default", // for more examples, look at: https://codemirror.net/demo/theme.html
            gutters: ['repl-prompt'],
            autoCloseBrackets: false,
            matchBrackets: true,
            // To highlight on scrollbars as well, pass annotateScrollbar in options as below.
            //highlightSelectionMatches: { showToken: /\w/, annotateScrollbar: true },
            scrollbarStyle: "simple" // or "overlay"
        }
    });

    var codemirrorContainers = document.getElementsByClassName('CodeMirror');
    for (let i = 0; i < codemirrorContainers.length; i++) {
        codemirrorContainers[i].style.height = (window.innerHeight - 100) + 'px';
    }    

    const language = 'C#';
    const scriptMode = 'script';
    const replMode = 'repl';
    SSE.msEditor.sendServerOptions({ 'language': language, 'x-mode': scriptMode });
    SSE.msRepl.sendServerOptions({ 'language': language, 'x-mode': replMode });

    window.addEventListener("resize", function (obj, e) {
        outputResults.style.height = (window.innerHeight - 100) + 'px';
        editorTextarea.style.height = (window.innerHeight - 100) + 'px';
        for (let i = 0; i < codemirrorContainers.length; i++) {
            codemirrorContainers[i].style.height = (window.innerHeight - 100) + 'px';
        }
    });
});