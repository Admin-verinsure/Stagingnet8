// Note this isn't included in build time, if you can please do so - didn't have time to do so.
// Use this wherever you use the build so you'd include in the page
// <script src="~/ckeditor/ckeditor5-techcertain/build/ckeditor.js"></script>
// <script src="~/ckeditor/ckeditor5-techcertain/build/selectimportimg.js"></script>

function selectImg(image) {
    var imgName = image.getAttribute('name');
    $('#selImg').val(imgName);
}

function importImg() {
    var triggerMutationObserver = $('#importImg').val();
    triggerMutationObserver += "1";
    $('#importImg').val(triggerMutationObserver);
}