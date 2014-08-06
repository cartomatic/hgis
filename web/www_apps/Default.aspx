<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>

<!DOCTYPE html>

<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title></title>
</head>
<body>
    <iframe id="iframe_downloader" width="0" height="0" src="proxies/empty.html" frameborder="0" scrolling="no" style="position: absolute; visibility: none; top: -100px; left: -100px;"></iframe>
    <a id="link_downloader" href="proxies/empty.html" style="position: absolute; visibility: none; top: -100px; left: -100px;"></a>
    <script type="text/javascript">
        document.getElementById('iframe_downloader').src = "proxies/empty.html";
    </script>
</body>
</html>
