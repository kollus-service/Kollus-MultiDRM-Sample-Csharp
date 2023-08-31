<%@ Page Language="C#" AutoEventWireup="true" CodeFile="Default.aspx.cs" Inherits="_Default" %>
<!DOCTYPE html>
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
<meta http-equiv="Content-Type" content="text/html; charset=utf-8"/>
    <script src="https://code.jquery.com/jquery-1.11.3.js"></script>
    <script src="https://file.kollus.com/vgcontroller/vg-controller-client.latest.min.js"></script> 
    <script>
        window.onload = function () {
            var controller = new Kollus.VideogatewayController({
                target_window: document.getElementById('video').contentWindow
            });
            controller.set_playback_rates("[0.5,1,1.5,2,2.5,3,3.5,4]"); // set playback speed rate
            controller.on('done', function () {
                document.getElementById("video").src = document.getElementById("nextVideo").value; // next contents play logic
            });

            document.getElementById('video').addEventListener('load', function () { //iframe loaded event, keep v/g controller code after next video loaded
                var controller2 = new Kollus.VideogatewayController({
                    target_window: document.getElementById('child').contentWindow
                });
                controller2.set_playback_rates("[0.5,1,1.5,2,2.5,3,3.5,4]");
                controller2.on('done', function () {
                    document.getElementById("video").src = document.getElementById("nextVideo").value;
                });
            });
        };
    </script>
    <title>Kollus MutiDRM Sample for C#</title>
</head>
<body>
    <form id="form1" runat="server">
        <div>
            <iframe runat="server" id="video" width="800" height="600" allowfullscreen webkitallowfullscreen mozallowfullscreen allow="encrypted-media"></iframe>
            <input type="hidden" runat="server" value="" id="nextVideo" />
        </div>
    </form>
</body>
</html>