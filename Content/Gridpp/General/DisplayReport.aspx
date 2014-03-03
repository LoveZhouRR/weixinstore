<%@ page language="C#" autoeventwireup="true" %>
<html xmlns="http://www.w3.org/1999/xhtml" >
<head runat="server">
	<title>Web报表(B/S报表)演示, 用报表查询显示器插件展现报表 - <%=Request.QueryString["Report"]%></title>
	<script src="../Scripts/CreateControl.js" type="text/javascript"></script>
    <style type="text/css">
        html,body {
            margin:0;
            height:100%;
        }
    </style>
</head>
<body style="margin:0">
	<script type="text/javascript"> 
	    var Report = "<%=Request.QueryString["Report"]%>";
	    if (Report != "")
	        Report = "../grf/" + Report;
	        
	    var Data = "<%=Request.QueryString["Data"]%>";
	    if (Data != "")
	        Data = "../" + Data;
	        
	    CreateDisplayViewerEx("100%", "100%", Report, Data, true, "");
	</script>
</body>
</html> 
