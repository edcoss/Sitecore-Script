<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ScriptEditor.aspx.cs" Inherits="Sitecore.Script.ScriptEditor" %>

<!DOCTYPE html>

<html>
<head runat="server">
    <title>Sitecore Script</title>
    <meta charset="utf-8">
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">
      <link rel="stylesheet" href="css/codemirror.css">
      <link rel="stylesheet" href="css/lint.css">
      <link rel="stylesheet" href="css/show-hint.css">
      <link rel="stylesheet" href="css/lint-fix.css">
      <link rel="stylesheet" href="css/infotip.css">
      <link rel="stylesheet" href="css/mirrorsharp.css">
      <link rel="stylesheet" href="https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css" integrity="sha384-Vkoo8x4CGsO3+Hhxv8T/Q5PaXtkKtu6ug5TOeNV6gBiFeWPGFN9MuhOf23Q9Ifjh" crossorigin="anonymous">
      <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/jstree/3.2.1/themes/default/style.min.css" crossorigin="anonymous">
    <link rel="stylesheet" href="css/scripteditor.css">
</head>
<body style="height: 100%">
    <form id="form1" runat="server">
        <input type="hidden" name="scriptPath" id="scriptPathField" runat="server" />
        <div class="Top">
            <nav class="navbar navbar-expand-md navbar-dark bg-dark">
                <div class="col-md-4 logo-col">
                    <div class="logo"></div>
                    <div>Sitecore Script</div>
                </div>
            
                <div class="col-md-10">           
                    <div class="collapse navbar-collapse" id="navbarCollapse">
                        <ul class="navbar-nav mr-auto">
						    <li class="nav-item active">
							    <asp:LinkButton ID="Roslyn" runat="server" OnClick="Run_Click" Text="Run" CausesValidation="False" CssClass="nav-link run" />
						    </li>
						    <li class="nav-item">
							    <asp:LinkButton ID="ResetRoslyn" runat="server" OnClick="ResetRun_Click" Text="Reset" CausesValidation="False" CssClass="nav-link reset" />
						    </li>
                            <li class="nav-item">
                                <a id="loadScript" href="javascript:void(0);" class="nav-link load-script">Load</a>
                            </li>
                            <li class="nav-item">
                                <a id="saveScript" href="javascript:void(0);" class="nav-link save-script">Save</a>
                            </li>
                        </ul>
                    </div>
                </div>
            </nav>
        </div>

        <div class="container-fluid">
            <div class="row">
                <div class="col-md-12 main">
                    <div class="Container" >
                        <div class="Left labeled">
                            <div class="row label">
                                <div class="col-1 white-text">Code</div>
                                <div class="col-11 white-text"># <span id="scriptPath">Untitled script</span></div>
                            </div>
                            <div class="row">
                                <div id="editorArea" class="col-12 codeeditor">                                
                                    <textarea rows="2" cols="20" id="code" style="display: none;" name="code"><asp:Literal runat="server" ID="CodeLiteral"/></textarea>
                                </div>   
                            </div>
                        </div>                        
                        <div class="Right labeled">
                            <div class="row label">
                                <div class="col-md-1 white-text">Output</div>
                                <div class="offset-md-9 col-md-2 executionTime white-text">Executed in <asp:Literal ID="TimeExecution" runat="server" /> ms</div>                            
                            </div>
                            <div class="row">
                                <div id="outputContainer" class="col-12">
                                    <asp:Literal ID="OutputHTML" runat="server"></asp:Literal>
                                    <asp:TextBox ID="Output" runat="server" style="display: none;" TextMode="MultiLine" Enabled="False" />
                                </div>   
                            </div>
                        </div>      
                    </div>
                </div>
            </div>
        </div>

        <div id="loadScriptTreeModal" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="loadSitecoreScriptLabel">
          <div class="modal-dialog" role="document">
            <div class="modal-content">
              <div class="modal-header">
                <h4 class="modal-title" id="loadSitecoreScriptLabel">Load Sitecore Script</h4>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
              </div>
              <div class="modal-body">
                <div class="load-script-tree" data-rootid="{583A2C0D-CA21-4EF5-A34C-BA68D80C97AC}">
                </div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                <button type="button" class="btn btn-primary load-script-button">Load</button>
              </div>
            </div><!-- /.modal-content -->
          </div><!-- /.modal-dialog -->
        </div><!-- /.modal -->

        <div id="saveScriptTreeModal" class="modal fade" tabindex="-1" role="dialog" aria-labelledby="loadSitecoreScriptLabel">
          <div class="modal-dialog" role="document">
            <div class="modal-content">
              <div class="modal-header">
                <h4 class="modal-title" id="saveSitecoreScriptLabel">Save Sitecore Script</h4>
                <button type="button" class="close" data-dismiss="modal" aria-label="Close"><span aria-hidden="true">&times;</span></button>
              </div>
              <div class="modal-body">
                <div class="save-script-title">
                    <label>Select script item or parent node:</label>
                </div>
                <br />
                <div class="save-script-tree" data-rootid="{583A2C0D-CA21-4EF5-A34C-BA68D80C97AC}">
                </div>
                <br />
                <div class="save-script-field">
                    <label>Script name:</label>
                    <input id="scriptNameField" type="text" placeholder="Type name here..." autocomplete="on" required maxlength="50" pattern="[\w\s]+"/>
                </div>
                <div class="save-error-message"></div>
              </div>
              <div class="modal-footer">
                <button type="button" class="btn btn-default" data-dismiss="modal">Close</button>
                <button type="button" class="btn btn-primary save-script-button">Save</button>
              </div>
            </div><!-- /.modal-content -->
          </div><!-- /.modal-dialog -->
        </div><!-- /.modal -->

    <script src="https://code.jquery.com/jquery-3.4.1.js" integrity="sha256-WpOohJOqMqqyKL9FccASB9O0KwACQJpFTUBLTYOVvVU=" crossorigin="anonymous"></script>
    <script src="https://cdn.jsdelivr.net/npm/popper.js@1.16.0/dist/umd/popper.min.js" integrity="sha384-Q6E9RHvbIyZFJoft+2mJbHaEWldlvI9IOYy5n3zV9zzTtmI3UksdQRVvoxMfooAo" crossorigin="anonymous"></script>
    <script src="https://stackpath.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js" integrity="sha384-wfSDF2E50Y2D1uUdj0O3uMBJnjuUD4Ih7YwaYd1iqfktj0Uod8GCExl3Og8ifwB6" crossorigin="anonymous"></script>
    <script src="js/jstree.js"></script>
    <script src="js/codemirror.js"></script>
    <script src="js/clike.js"></script>
    <script src="js/lint.js"></script>
    <script src="js/show-hint.js"></script>
    <script src="js/lint-fix.js"></script>
    <script src="js/infotip.js"></script>
    <script src="js/mirrorsharp.js"></script>
    <script src="js/scripteditor.js"></script>
    </form>
  </body>
</html>
