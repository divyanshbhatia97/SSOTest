<%@ Page Title="About" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="About.aspx.cs" Inherits="SingleSignOn.About" %>

<asp:Content ID="BodyContent" ContentPlaceHolderID="MainContent" runat="server">
    <h2><%: Title %></h2>
    <p><a href="https://en.wikipedia.org/wiki/Single_sign-on">Single sign-on (SSO)</a> is a property of access control of multiple related, yet independent,
      software systems. With this property, a user logs in with a single ID and password to gain access 
      to any of several related systems. It is often accomplished by using the Lightweight Directory Access
      Protocol (LDAP) and stored LDAP databases on (directory) servers.A simple version of single sign-on 
      can be achieved over IP networks using cookies but only if the sites share a common DNS parent domain.</p>
</asp:Content>