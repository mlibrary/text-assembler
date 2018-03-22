<%-- 
COPYRIGHT  2015-16
MICHIGAN STATE UNIVERSITY BOARD OF TRUSTEES
ALL RIGHTS RESERVED

PERMISSION IS GRANTED TO USE, COPY, CREATE DERIVATIVE WORKS AND REDISTRIBUTE
THIS SOFTWARE AND SUCH DERIVATIVE WORKS FOR ANY PURPOSE, SO LONG AS THE NAME
OF MICHIGAN STATE UNIVERSITY IS NOT USED IN ANY ADVERTISING OR PUBLICITY
PERTAINING TO THE USE OR DISTRIBUTION OF THIS SOFTWARE WITHOUT SPECIFIC,
WRITTEN PRIOR AUTHORIZATION.  IF THE ABOVE COPYRIGHT NOTICE OR ANY OTHER
IDENTIFICATION OF MICHIGAN STATE UNIVERSITY IS INCLUDED IN ANY COPY OF ANY
PORTION OF THIS SOFTWARE, THEN THE DISCLAIMER BELOW MUST ALSO BE INCLUDED.

THIS SOFTWARE IS PROVIDED AS IS, WITHOUT REPRESENTATION FROM MICHIGAN STATE
UNIVERSITY AS TO ITS FITNESS FOR ANY PURPOSE, AND WITHOUT WARRANTY BY
MICHIGAN STATE UNIVERSITY OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
WITHOUT LIMITATION THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
A PARTICULAR PURPOSE. THE MICHIGAN STATE UNIVERSITY BOARD OF TRUSTEES SHALL
NOT BE LIABLE FOR ANY DAMAGES, INCLUDING SPECIAL, INDIRECT, INCIDENTAL, OR
CONSEQUENTIAL DAMAGES, WITH RESPECT TO ANY CLAIM ARISING OUT OF OR IN
CONNECTION WITH THE USE OF THE SOFTWARE, EVEN IF IT HAS BEEN OR IS HEREAFTER
ADVISED OF THE POSSIBILITY OF SUCH DAMAGES.

Written by Megan Schanz, 2015-16
(c) Michigan State University Board of Trustees
Licensed under GNU General Public License (GPL) Version 2.
--%>
<%@ Page Title="Search" Language="C#" AutoEventWireup="true" CodeBehind="SearchForm.aspx.cs" Inherits="LexisNexisWSKImplementation.SearchForm"   EnableViewState="true" MasterPageFile="site.master" %>

<asp:Content ID="Content2" ContentPlaceHolderID="userPlaceholder" Runat="Server">
    <!-- Displays the currently logged in user ID -->
    <p> <span runat="server" id="user_label" /> </p> 
</asp:Content>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <!-- Script used to maintain the current tab on postback -->


    <script type="text/javascript">
        $(function () {
            $('[id*=lstFolders]').multiselect({
                includeSelectAllOption: true,
                maxHeight: 300,
                buttonWidth: '400px',
                enableCaseInsensitiveFiltering: true,
                nonSelectedText: 'No Publication Type(s) Selected',
                includeFilterClearBtn: false
            });
            $('[id*=lstSources]').multiselect({
                includeSelectAllOption: false,
                maxHeight: 300,
                buttonWidth: '400px',
                enableCaseInsensitiveFiltering: true,
                disableIfEmpty: true,
                includeFilterClearBtn: false,
                nonSelectedText: 'No Title(s) Selected'
            });
            $('[id*=txtFrom]').datepicker({ dateFormat: 'mm/dd/yy' });
            $('[id*=txtTo]').datepicker({ dateFormat: 'mm/dd/yy' });
        });
</script>

    <script>
        $(document).ready(function () {
            $('[data-toggle="tooltip"]').tooltip();
        });

    </script>

    <script type="text/javascript">
        $(function () {
            var tabName = $("[id*=TabName]").val() != "" ? $("[id*=TabName]").val() : "search";
            $('#Tabs a[href="#' + tabName + '"]').tab('show');
            $("#Tabs a").click(function () {
                try {
                    $("[id*=TabName]").val($(this).attr("href").replace("#", ""));
                }
                catch (err) { }
            });
        });
       
    </script>
    
    <div id="Tabs" role="tabpanel" >
        <!-- Navigation Bar -->  
       <ul class="nav nav-tabs nav-justified" role="tablist">
            <li class="active"><a data-toggle="tab" href="#search" aria-controls="search" role="tab">Search</a></li>
            <li><a data-toggle="tab" href="#user" aria-controls="user" role="tab">My Corpus Download Queue</a></li>
           <li><a data-toggle="tab" href="#logout" aria-controls="logout" role="tab"><span class="glyphicon glyphicon-log-out"></span>Logout</a></li>
       </ul>
        <div class="padBody">
      <div class="tab-content">

          <!-- Search Tab -->
        <div id="search" class="tab-pane fade in active" role="tabpanel">
            <h2>Search</h2>
            <p><b class="lnLabel">Instructions: </b>Select source(s), construct search, and preview results. If preview results look 
                appropriate, assign a Corpus Name and queue for download. Downloading is queued for nights and weekends. Please plan ahead to ensure 
                sufficient time to retrieve results. For more guidance consult the <a href="#" data-toggle="modal" data-target="#myModal" style="color:#0000FF;border-bottom: 1px solid #0000FF;">
                    step by step instructions.</a>
                For more information on how to construct more effective searches see <asp:LinkButton ID="lnkDownloadTips" runat="server" OnClick="lnkDownloadTips_Click" Text="these tips" style="color:#0000FF;border-bottom: 1px solid #0000FF;"/>.
            </p> 
                        
            <table><tr><td><b class="lnLabel">Available Sources: </b></td>
                <td>
                <label id="SourceFolders" class="noFormatLabel"><asp:ListBox runat="server" ID="lstFolders" SelectionMode="multiple"></asp:ListBox></label></td>
                    <td style="padding-left:5px"><asp:Button ID="btnFilter" runat="server" Text="Apply Publication Type Filter" class="smallButtonStyle" onclick="btnFilter_Click" /></td></tr>
                <tr><td></td><td>
                <label id="Sources" class="noFormatLabel"><asp:ListBox runat="server" ID="lstSources" SelectionMode="multiple"></asp:ListBox> </label></td>
                    <td style="padding-left:5px"><asp:Button ID="btnAdd" runat="server" Text="Add Titles" class="smallButtonStyle" onclick="btnAdd_Click" />
                       </td>
                </tr>
                <tr><td><asp:Label id="labelSelectedTitles" AssociatedControlID="txtSources" runat="server" Text="Selected Title Ids: " CssClass="lnLabel" /></td><td><asp:TextBox ID="txtSources"  style="width:400px;" runat="server" />
                    </td></tr>
            </table>


                <table>
                
                    <tr>
                         <td><b class="lnLabel">Search Method:  (<a href="#" data-toggle="modal" data-target="#searchModal" style="color:#0000FF;border-bottom: 1px solid #0000FF;">?</a>)</b></td>
                        <td><label id ="SearchMethods" class="noFormatLabel"><asp:DropDownList ID="lstMethods" runat="server" style="width:300px;margin-left:auto;margin-right:auto"></asp:DropDownList>&nbsp</label></td>                        
                    </tr>
                    <tr>
                        <td><b class="lnLabel">Date Range: </b> </td>
                        <td>                  
                            <table><tr><td><asp:Label  id="labelFrom" AssociatedControlId="txtFrom"  runat="server" Text="From: " CssClass="noFormatLabel"/></td><td><asp:TextBox ID="txtFrom" runat="server"  style="width:100px;margin-left:auto;margin-right:auto" /></td>
                                    <td><asp:Label  id="labelTo" AssociatedControlId="txtTo"  runat="server" Text="To: " CssClass="noFormatLabel"/></td><td><asp:TextBox ID="txtTo" runat="server" style="width:100px;margin-left:auto;margin-right:auto" /></td>
                                <td><div class="checkbox">
                                    <%-- Commenting out since this can lead to dangerously large searches --%>
<%--                              &nbsp&nbsp<label id="NoDateRange" class="noFormatLabel"><asp:CheckBox ID="cbDateRange" runat="server"/>No Date Range</label>    --%>                           
                            </div></td>
                                   </tr>
                            </table>
                                                   
                        </td>  
                    </tr>
                    <tr>
                        <td><asp:Label id="labelSearchQuery" AssociatedControlID="txtSearch" runat="server" Text="Search Query: " CssClass="lnLabel" /></td>
                        <td><asp:TextBox ID="txtSearch" runat="server" TextMode="multiline" style="width:300px;height:100px;" /> </td>                        
                     </tr>
                    <tr><td style="padding-left: 5px"><asp:Button ID="btnSearch" runat="server" Text="Preview Results" class="buttonStyle no-modal" onclick="btnSearch_Click" /></td></tr>
                    <tr>   
			<td><asp:Label id="labelSearchName" AssociatedControlID="txtSearchName" runat="server" Text="Your Corpus Name: " CssClass="lnLabel" /></td>   
			<td><asp:TextBox ID="txtSearchName"  runat="server" />        
			&nbsp&nbsp<asp:Button ID="btnQueue" runat="server" Text="Queue Corpus for Download" class="buttonStyle" onclick="btnQueueSearch_Click" /></td>
                    </tr>
               
                </table>    
            <div id="divWarning" runat="server" visible="false">
                <p class="warningLbl">The search you have entered is using more than 1,000 sources and will most likely exceed the source limit. Most source categories have group sources available
                    the contain multiple sources in one record, please review the "Commonly Used Resources" for some examples of these. Do you wish to continue with your current search
                    criteria or cancel and refine your search?
                </p>
                <table>
                <tr><td style="padding-left:60px;"><asp:Button id="btnConfirm" runat="server" Text="Continue" class="smallButtonStyle" OnClick="btnConfirm_Click" /> 
                <asp:Button ID="btnCancel" runat="server" Text="Cancel" CssClass="smallButtonStyle" OnClick="btnCancel_Click" /></td></tr>
                    </table>
            </div>   
                <hr>
            <h2>Results: </h2>

                 <!-- Shows results or errors that occur during form processing -->
                 <span runat="server" id="result_text" />

            </div>
   

          <!-- Instruction Modal -->
          <div class="modal fade" id="myModal" role="dialog">
            <div class="modal-dialog">
    
              <!-- Modal content-->
              <div class="modal-content">
                <div class="modal-header">
                  <button type="button" class="close" data-dismiss="modal">&times;</button>
                  <h2 class="modal-title">Instructions</h2>
                </div>
                <div class="modal-body">
                  <ol>
                      <li class="narrowP">Select publication type(s) from the drop down and click ‘Apply Publication Filter’</li>
                      <li class="narrowP">Select title(s) from the drop down and click ‘Add Title(s)’</li>
                      <li class="narrowP">Select a search method from the drop down</li>
                      <li class="narrowP">Provide date range</li> 
                      <li class="narrowP">Construct search</li>
                      <li class="narrowP">Preview results</li>
                      <li class="narrowP">Refine search as needed</li>
                      <li class="narrowP">Assign Corpus Name</li>
                      <li class="narrowP">Queue corpus for download</li>
                      <li class="narrowP">Retrieve corpus from ‘My Corpus Download Queue’</li> 
                  </ol>
                </div>
                <div class="modal-footer">
                  <button type="button" class="buttonStyle" data-dismiss="modal">Close</button>
                </div>
              </div>
      
            </div>
          </div>

           <!-- Search Instruction Modal -->
          <div class="modal fade" id="searchModal" role="dialog">
            <div class="modal-dialog modal-lg">
    
              <!-- Modal content-->
              <div class="modal-content">
                <div class="modal-header">
                  <button type="button" class="close" data-dismiss="modal">&times;</button>
                  <h2 class="modal-title">Search Methods</h2>
                </div>
                <div class="modal-body">
                    <p style="line-height:1.2em;">Lexis Nexis supports both Boolean and Natural Language searching.  The following is a brief description of the characteristics of each method.
                    <br /><br />For information on developing a Boolean Search, please see <a href="https://www.lexisnexis.com/Webserviceskit/Developers/v2_0beta/text/LN_help/gh_terms.htm" style="color:#0000FF;border-bottom: 1px solid #0000FF;">Developing a Search (with Terms and Conditions)</a>
                    <br />For information on developing a Natural Language Search, please see <a href="https://www.lexisnexis.com/Webserviceskit/Developers/v2_0beta/text/LN_help/gh_natural.htm" style="color:#0000FF;border-bottom: 1px solid #0000FF;">Developing a Search (with Natural Language)</a></p> 
                    <table class="methodTbl">
                        <thead><tr><th>Search Method</th><th>Description</th></tr></thead>
                          <tr>
                          <td>Boolean</td>
                          <td>Boolean searching in the LexisNexis query languages uses a rich set of terms and connectors.  For more details on the Boolean terms and connectors available,  see the links at the top.
                                <br /><br />Boolean searches are subject to a maximum number of results that can be returned in an answerset.
                                <br /><br />If the search would return more than the maximum results (3000) then the error  "SEARCH_TOO_GENERAL" is returned and the Search is interrupted.
                                <br /><br />In this case, no results are returned, and it is up to the user to re-submit the search with more precise terms and/or a shorter date range to ensure the query returns fewer than the limit.
                          </td></tr>

                          <tr><td>MatchOnAllWords</td>
                          <td>Similar to the MatchOnPhrase search, except that all of the query terms must appear in the document for it to appear in the result list. </td></tr>

                          <tr><td>MatchOnAnyWord</td>
                          <td>Similar to the MatchOnPhrase search, but with the dynamic phrase option set to not affect the scoring of the documents in the result list.</td></tr>

                          <tr><td>MatchOnPhrase</td>
                          <td>Similar to the Freestyle search, except that at least one of the query terms must appear in the document for it to appear in the result set.</td></tr>

                          <tr><td>QuickSearch</td>
                          <td>Similar to the MatchOnPhrase search, but with the dynamic phrase option set to not affect the scoring of the documents in the result list.</td></tr>

                          <tr><td>Freestyle</td>
                          <td>Freestyle searching in the LexisNexis query languages allows you to search in plain English, without the need for any special terms or connectors. Natural language document relevance ranking gives you quick access to the most pertinent documents in your search results.
                                <br /><br />Natural Language searching uses a phrase dictionary to dynamically identify commonly used legal and business phrases in the query string. Additionally phrases can be manually specified in the query by enclosing them in double quotes. Phrases help the relevance ranking algorithms determine the scoring for each result, and pushing the most relevant results to the top of the search results.
                                <br /><br />Freestyle natural language doesn’t require the search terms from the query string to be present in the documents returned from the query. However the documents will all be relevant to the concepts extracted from the query test by the natural language processor.
                                <br /><br />The Freestyle option matches the Natural Language search option available in the Nexis.com and Nexis UK  products.
                          </td></tr>

                          <tr><td>Raw</td>
                          <td>Does not use either natural language search terms or boolean terms and connectors and allows raw searches to be performed.</td></tr>
                  </table>
                </div>
                <div class="modal-footer">
                  <button type="button" class="buttonStyle" data-dismiss="modal">Close</button>
                </div>
              </div>
      
            </div>
          </div>


        
          <!-- User Tab -->
        <div id="user" class="tab-pane fade" role="tabpanel">
            <!--<p class='warningLbl'>The queue processing system is down for temporarily for maintenance.</p>-->
            <h2>Corpus Download Queue</h2>
            <p class="center"> <span runat="server" id="runWindow" /> </p>
            <table class="center"><tr><td>
                <asp:Button ID="btnRefresh" runat="server" Text="Refresh Data" class="buttonStyle" onclick="btnRefresh_Click" />
            </td></tr></table>

            <asp:ListView runat="server" ID="myListView">
            <LayoutTemplate>
                <table class="results">
                     <colgroup>
                        <col span="1" style="width: 15%;"/>
                        <col span="1" style="width: 15%;"/>
                        <col span="1" style="width: 16%;"/>                     
                        <col span="1" style="width: 7%;"/>
                        <col span="1" style="width: 10%;"/>
                        <col span="1" style="width: 7%;"/>
                        <col span="1" style="width: 20%;"/>
                        <col span="1" style="width: 10%;"/>
                    </colgroup>
                    <thead >
                        <tr >
                            <th class="header">Corpus Name</th>
                            <th class="header">Query</th>
                            <th class="header">Date Queued</th>
                            <th class="header">Queue Position</th>
                            <th class="header">Status</th>
                            <th class="header">Number of Results</th>
                            <th class="header">Percent Complete</th>
                            <th class="header">Action</th>
                        </tr>
                    </thead>
                    <tbody style="overflow:auto">
                        <asp:PlaceHolder ID="itemPlaceHolder" runat="server" />
                    </tbody>
                </table>
            </LayoutTemplate>
            <ItemTemplate>
                <tr>
                    <td class="results"><%# Eval("searchName") %></td>
                    <td class="results"><%# Eval("searchQuery") %></td>
                    <td class="results"><%# Eval("searchDate") %></td>
                    <td class="results"><%# Eval("searchQueuePosition") %></td>
                    <td class="results"><%# Eval("searchStatus") %></td>   
                    <td class="results"><%# Eval("searchResultCount") %></td>   
                    <td class="results">
                        <div class="progress">
                          <div ID="progressBar" runat="server" class="progress-bar progress-bar-success progress-bar" role="progressbar" aria-valuenow='<%# Eval("searchPercent") %>'
                          aria-valuemin="0" aria-valuemax="100" style='<%# "width:" +  Eval("searchPercent") + "%" %>'>
                            <%# Eval("searchPercent") %>%
                          </div>
                        </div>
                    </td>
                    <td class="results">
                        <asp:LinkButton Text='<%# Eval("searchAction") %>' Visible='<%# !Eval("searchAction").ToString().Equals(String.Empty) %>'  ID="lkbCommandAction" CommandArgument='<%# Eval("searchID") %>' OnCommand="lkbCommandAction_Command" runat="server" causesvalidation="false" style="color:blue"/>
                    </td>
                </tr>
            </ItemTemplate>
        </asp:ListView>
             <hr />
            <p> <span runat="server" id="userTab_results" /> </p>
        </div>

          <!-- Logout Tab -->
        <div id="logout" class="tab-pane fade" role="tabpanel">
            <table><tr><td>
            <b class="lnLabel" >Are you sure you sure you want to log out?</b>
                </td></tr><tr><td>
            <asp:Button ID="btnLogout2" runat="server" Text="Logout" onclick="btnLogout_Click" class="buttonStyle"/>
                    </td></tr></table>
        </div>
       
      </div>
   </div>
        </div>

  <!-- Used to store the currently selected tab  -->
  <asp:HiddenField ID="TabName" runat="server" />  

</asp:Content>

<asp:Content ID="Content3" ContentPlaceHolderID="errorPlaceholder" Runat="Server">
    <!-- Any page errors will be put in this container -->
    <span runat="server" id="error_label" />
  
</asp:Content>
