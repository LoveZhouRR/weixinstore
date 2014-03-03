<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="ExpressPrint.aspx.cs" Inherits="DBC.Ors.UI.Web.Mvc.ERP.Content.Gridpp.XmlData.ExpressPrint" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <title></title>
    <script type="text/javascript">
        function PrintPage() {
            window.print();
            window.close();
        }
    </script>
</head>
<body onload="<%=OnloadFunction %>" style="margin:0">
    <form id="form1" runat="server">
        <img id="imgExpress" runat="server" alt="" style="width:600px;height:400px" />
        <asp:Label ID="lblMessage" runat="server" Text="" Visible="false"></asp:Label>
    </form>
</body>
</html>
