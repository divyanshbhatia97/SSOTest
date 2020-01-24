<%@ Page Title="Home Page" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Default.aspx.cs" Inherits="SingleSignOn._Default" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">

    <div class="jumbotron">
        <h1>Single-Sign On</h1>
        <p class="lead">Single sign-on (SSO) is a property of access control of multiple related, yet independent, software systems.</p>
        <asp:Button runat="server" ID="btnTestSSO" Text="Test SSO &raquo;" CssClass="btn btn-primary btn-lg" OnClick="btnTestSSO_Click" />  
      <%--<p><a href="test.aspx" class="btn btn-primary btn-lg">Test SSO &raquo;</a></p>--%>
    </div>

    <div class="row">
        <div class="col-md-4">
            <h2>Getting started</h2>
            <p>
                This application is used to test SSO functionality.
            </p>
        </div>
    </div>

</asp:Content>
