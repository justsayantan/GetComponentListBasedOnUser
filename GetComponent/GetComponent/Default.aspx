<%@ Page Title="Home Page" Language="C#" CodeBehind="Default.aspx.cs" Inherits="GetComponent._Default" %>

<html>
<head>
    <title="Get Component List Baser On User" />
</head>
<body>
    <h1>Choose User from Dropdownlist</h1>
    <form runat="server">
        <div>
            <h2>User : </h2>
            <asp:DropDownList ID="userddList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="userddList_SelectedIndexChanged"></asp:DropDownList>
            <h2>Publication : </h2>
            <asp:DropDownList ID="publicationddList" runat="server" AutoPostBack="true" OnSelectedIndexChanged="publicationList_SelectedIndexChanged"></asp:DropDownList>
            <asp:Button ID="DownloadComponentList" runat="server" Text="Download List" OnClick="DownloadComponentList_Click" />

        </div>
        <div style="margin-top: 49px">
            <asp:Label ID="LabelMessage" runat="server"></asp:Label>
        </div>
    </form>
</body>
</html>

