/// <reference path="../typings/knockout/knockout.d.ts" />
ko.options.deferUpdates = true;
function toCurrency(value) {
    return value.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}
var Value = function (val) {
    this.number = ko.observable(Math.round(val));
};
var data1 = [];
var area1 = [];
var Entry = (function () {
    function Entry(v) {
        this.value = v;
    }
    return Entry;
}());
var getEntry = function (tagValues) {
    var entryList = tagValues.row;
    var total = entryList.entries.reduce(function (prev, curr) { return (prev + curr); }, 0);
    return {
        entries: entryList.entries.map(function (x) { return ko.observable(new Entry(x)); }),
        //entries: ko.observableArray(entryList.entries),
        //entries: ko.observableArray(entryList.entries.map(x => ko.observable(x))),
        tag: tagValues.tag,
        mean: tagValues.row.mean,
        total: total,
        meanCurr: toCurrency(entryList.mean),
        totalCurr: toCurrency(total),
        entryType: total > 0 ? "success" : "danger"
    };
};
var VM = function (entries) {
    var _this = this;
    var self = this;
    self.labels = [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep",
        "Oct", "Nov", "Dic"
    ];
    self.entries = entries.map(function (elm) { return getEntry(elm); });
    var totalIncome = ko.computed(function () { return _this.entries.reduce(function (prev, curr) {
        if (curr.total > 0)
            return prev + curr.total;
        else
            return prev;
    }, 0); });
    var totalExpence = ko.computed(function () { return _this.entries.reduce(function (prev, curr) {
        if (curr.total <= 0)
            return prev + curr.total;
        else
            return prev;
    }, 0); });
    var totalMeanIncome = ko.computed(function () { return _this.entries.reduce(function (prev, curr) {
        if (curr.mean > 0)
            return prev + curr.mean;
        else
            return prev;
    }, 0); });
    var totalMeanExpence = ko.computed(function () { return _this.entries.reduce(function (prev, curr) {
        if (curr.mean <= 0)
            return prev + curr.mean;
        else
            return prev;
    }, 0); });
    var remaining = ko.computed(function () { return (totalIncome() + totalExpence()); });
    self.totalIncomeCurr = ko.computed(function () { return toCurrency(totalIncome()); });
    self.totalExpenceCurr = ko.computed(function () { return toCurrency(totalExpence()); });
    self.totalMeanIncomeCurr = ko.computed(function () { return toCurrency(totalMeanIncome()); });
    self.totalMeanExpenceCurr = ko.computed(function () { return toCurrency(totalMeanExpence()); });
    self.remainingCurr = ko.computed(function () { return toCurrency(remaining()); });
    self.profitStatus = ko.computed(function () { return (remaining() > 0 ? "success" : "danger"); });
    self.Data = {
        labels: self.labels,
        datasets: self.dataset1
    };
};
