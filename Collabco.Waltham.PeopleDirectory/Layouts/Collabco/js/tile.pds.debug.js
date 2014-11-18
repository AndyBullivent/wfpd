function spinPeopleDirectoryTile(uid, title) {
    //startSpinning("pds", uid);

    //setTimeout(function () {
        setupPeopleDirectoryTile(uid, title);
        //stopSpinning("pds", uid);
    //}, 1000);
}

function setupPeopleDirectoryTile(uid, title) {
    try {
        if (determinePageState() == "live") {

            var urlParts = document.createElement('a');
            urlParts.href = document.URL;

            var searchURL = urlParts.protocol + "//" + urlParts.hostname + "/_layouts/15/collabco.waltham.peopledirectory/search.aspx";
            var tileContents = "";

            
            tileContents += "    <div id='pds'><br/>";
            tileContents += "    <div class='attenSlider_container'>";
            tileContents += "       <a href='" + searchURL + "' target=_blank>";
            tileContents += "        <h5 class='sliderTitle'>" + title + "</h5>";
            tileContents += "       </a>";
            tileContents += "        <input type=\"text\" id=\"searchtextbox\" placeholder=\"Search...\"onkeydown=\"if (event.keyCode == 13) { window.location.href = '" + searchURL + "?searchText='+this.value; return false; }\"/>";
            tileContents += "    </div>";
            tileContents += "</div>";
            
            jQuery("#hubtile-pds-" + uid + " .hubslider_container").html(tileContents);
            jQuery("#hubtile-pds-" + uid + " .hubtiletitle").text(" "); // setting the webpart tile title blank
            makeTileDynamic("pds", uid);
                    
        }
    }
    catch (e) {
        console.log("tile.students > setupStudentsTile() :" + e);
        makeTileError("pds", uid);
    }
}
