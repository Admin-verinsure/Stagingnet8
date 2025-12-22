//-------------------------------------------------------
//  NUMERIC VALIDATION (NO LETTERS)
//-------------------------------------------------------
$(document).on("keypress", ".member-count, .district-count, .spt-count", function (e) {
    if (!/[0-9]/.test(e.key)) {
        e.preventDefault();
    }
});

// BLOCK NON-NUMERIC PASTE
$(document).on("paste", ".member-count, .district-count, .spt-count", function (e) {
    let paste = (e.originalEvent || e).clipboardData.getData('text');
    if (!/^\d+$/.test(paste)) {
        e.preventDefault();
    }
});


//-------------------------------------------------------
//  CLUB MEMBER TOTAL — LIVE UPDATE
//-------------------------------------------------------
$(document).on("input", ".member-count", function () {

    let total = 0;

    $(".member-count").each(function () {
        let val = parseInt($(this).val()) || 0;

        // Corporate counts × 3
        if (this.id === "Corporate") {
            total += val * 3;
        } else {
            total += val;
        }
    });

    $("#TotalMembers").text(total);
    $("#ClubTotal").val(total);   // SAVE TO HIDDEN FIELD
});


//-------------------------------------------------------
//  DISTRICT TOTAL — LIVE UPDATE
//-------------------------------------------------------
$(document).on("input", ".district-count", function () {

    let total = 0;

    $(".district-count").each(function () {
        let val = parseInt($(this).val()) || 0;
        total += val;
    });

    $("#DistrictTotal").text(total);
    $("#DistrictTotalHidden").val(total);  // SAVE TO HIDDEN FIELD
});


//-------------------------------------------------------
//  SPECIAL PURPOSE TRUST — LIVE UPDATE
//-------------------------------------------------------
$(document).on("input", ".spt-count", function () {

    let companies = parseInt($("#SPT_Companies").val()) || 0;
    let trusts = parseInt($("#SPT_TradingTrusts").val()) || 0;

    let total = companies + trusts;

    $("#SPT_Total").text(total);
    $("#SPT_TotalHidden").val(total);   // SAVE TO HIDDEN FIELD
});


//-------------------------------------------------------
//  SHOW/HIDE SPT REVENUE FIELD
//-------------------------------------------------------
$(document).on("change", "#SPT_RevenueOver1m", function () {

    if ($(this).val() === "YES") {
        $("#SPT_RevenueRow").show();
    } else {
        $("#SPT_RevenueRow").hide();
        $("#SPT_Revenue").val("");
    }
});


//-------------------------------------------------------
//  INITIAL TOTAL CALCULATION — ON PAGE LOAD (EDIT MODE)
//-------------------------------------------------------
$(document).ready(function () {

    // ----- CLUB INITIAL TOTAL -----
    let clubTotal = 0;
    $(".member-count").each(function () {
        let val = parseInt($(this).val()) || 0;
        if (this.id === "Corporate") clubTotal += val * 3;
        else clubTotal += val;
    });
    $("#TotalMembers").text(clubTotal);
    $("#ClubTotal").val(clubTotal);


    // ----- DISTRICT INITIAL TOTAL -----
    let distTotal = 0;
    $(".district-count").each(function () {
        let val = parseInt($(this).val()) || 0;
        distTotal += val;
    });
    $("#DistrictTotal").text(distTotal);
    $("#DistrictTotalHidden").val(distTotal);


    // ----- SPT INITIAL TOTAL -----
    let c = parseInt($("#SPT_Companies").val()) || 0;
    let t = parseInt($("#SPT_TradingTrusts").val()) || 0;
    let sptTotal = c + t;

    $("#SPT_Total").text(sptTotal);
    $("#SPT_TotalHidden").val(sptTotal);


    // ----- SHOW/HIDE REVENUE FIELD -----
    if ($("#SPT_RevenueOver1m").val() === "YES") {
        $("#SPT_RevenueRow").show();
    } else {
        $("#SPT_RevenueRow").hide();
    }
});
