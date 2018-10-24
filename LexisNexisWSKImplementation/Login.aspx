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
<%@ Page Title="Login" Language="C#" AutoEventWireup="true" CodeBehind="Login.aspx.cs" Inherits="LexisNexisWSKImplementation._Default" MasterPageFile="site.master" %>


<asp:Content ID="Content1" ContentPlaceHolderID="MainContent" Runat="Server">
    <div class="padBody">
        <h1 class="login">LexisNexis WSK Implementation Login</h1>
        <p class="narrowP"><br /><br />This LexisNexis WSK Implementation enables construction and bulk download of full text corpora derived from newspapers, newswires, web-based news publications, broadcast news transcripts, magazines, and legal and trade publications published between 1980 and the present. Consult the Lexis Nexis <a href="https://w3.nexis.com/sources/scripts/eslClient.pl?GSDTYPE=News" style="color:#0000FF;border-bottom: 1px solid #0000FF;">A-Z list of sources and dates</a> to orient yourself to available data. 
            For an overview of how Text Assembler functions, consult the <asp:LinkButton ID="lnkDownloadTech" runat="server" OnClick="lnkDownloadTech_Click" Text="technical documentation"  style="color:#0000FF;border-bottom: 1px solid #0000FF;"/>.
            <br /><br />In order to build a corpus, construct a search, review the search results, and queue all content matching your search criteria for download. Text Assembler outputs a ZIP file that contains an identical set of HTML and TXT files that match your search criteria, sorted in descending date order. Downloading is queued for nights and weekends. Please plan ahead to ensure sufficient time to retrieve results.
            <br /><br />This tool and data provided through it can only be used for educational and research purposes of authorized users.  
            <br /><br /><strong>Uses that are not allowed:</strong>
            </p>
            <ul><li class="narrowP">You may not use the database or any part of the information comprised in the database content for commercial research, for example, research that is done under a funding or consultant contract, internship, or other relationship in which the results are delivered to a for-profit organization.</li>
                <li class="narrowP">You may not sell or otherwise re-distribute data to third parties without express permission.</li>
                <li class="narrowP">You may not engage in bulk reproduction or distribution of the licensed materials in any form.</li>
                </ul>
            
            <hr />        
        <!-- Shows results or errors that occur during form processing -->
         <p> <span runat="server" id="result_text" /> </p>
</div>
</asp:Content>

