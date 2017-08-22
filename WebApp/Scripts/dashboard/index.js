/// <reference path="../typings/knockout/knockout.d.ts" />
ko.options.deferUpdates = true;
function toCurrency(value) {
    return value.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}
var Value = function (val) {
    this.number = ko.observable(Math.round(val));
};
var months = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep",
    "Oct", "Nov", "Dic"
];
var Entry = (function () {
    function Entry(v, month) {
        var _this = this;
        this.getValue = function () { return parseFloat(_this.value()); };
        this.value = ko.observable(v + "");
        this.month = months[month];
    }
    return Entry;
}());
var KnockoutEntry = (function () {
    function KnockoutEntry(tagValues) {
        var entryList = tagValues.row;
        var observableEntries = entryList.entries.map(function (x, i) { return new Entry(x, i); });
        var total = ko.pureComputed(function () { return observableEntries.reduce(function (prev, curr) {
            return (prev + curr.getValue());
        }, 0); });
        var mean = ko.pureComputed(function () {
            var sum = 0;
            var count = 0;
            var date = new Date();
            var month = date.getMonth();
            for (var i = 0; i < observableEntries.length; i++) {
                var val = parseFloat(observableEntries[i].value());
                if (val == 0 && i >= month) {
                    continue;
                }
                sum += val;
                count++;
            }
            return sum / count;
        });
        this.entries = observableEntries;
        this.tag = tagValues.tag;
        this.category = tagValues.category;
        this.mean = mean;
        this.total = total;
        this.meanCurr = ko.pureComputed(function () { return toCurrency(mean()); });
        this.expectedTotal = ko.pureComputed(function () { return mean() * 12; });
        this.totalCurr = ko.pureComputed(function () { return toCurrency(total()); });
        this.entryType = total() > 0 ? "success" : "danger";
    }
    KnockoutEntry.prototype.getEntryType = function (row1, id, vm) {
        debugger;
        if (id() == 0)
            return row1.total() > 0 ? "success" : "danger";
        if (row1.category == vm.entries[id() - 1].category)
            return row1.total() > 0 ? "success" : "danger";
        else
            return row1.total() > 0 ? "success separator" : "danger separator";
    };
    return KnockoutEntry;
}());
var VM = (function () {
    function VM(entries) {
        var _this = this;
        this.totalIncomeCurr = ko.pureComputed(function () { return toCurrency(_this.totalIncome()); });
        this.totalExpenceCurr = ko.pureComputed(function () { return toCurrency(_this.totalExpence()); });
        this.totalMeanIncomeCurr = ko.pureComputed(function () { return toCurrency(_this.totalMeanIncome()); });
        this.totalExpectedIncomeCurr = ko.pureComputed(function () { return toCurrency(_this.totalExpectedIncome()); });
        this.totalMeanExpenceCurr = ko.pureComputed(function () { return toCurrency(_this.totalMeanExpence()); });
        this.totalExpectedExpenceCurr = ko.pureComputed(function () { return toCurrency(_this.totalExpectedExpence()); });
        this.remainingCurr = ko.pureComputed(function () { return toCurrency(_this.remaining()); });
        this.profitStatus = ko.pureComputed(function () { return (_this.remaining() > 0 ? "success" : "danger"); });
        this.labels = months;
        this.entries = entries.map(function (elm) { return new KnockoutEntry(elm); });
    }
    VM.prototype.totalIncome = function () {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce(function (prev, curr) { return curr.total() > 0 ? prev + curr.total() : prev; }, 0);
    };
    VM.prototype.totalExpence = function () {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce(function (prev, curr) { return curr.total() <= 0 ? prev + curr.total() : prev; }, 0);
    };
    VM.prototype.totalMeanIncome = function () {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce(function (prev, curr) { return curr.mean() > 0 ? prev + curr.mean() : prev; }, 0);
    };
    VM.prototype.totalExpectedIncome = function () {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce(function (prev, curr) { return curr.expectedTotal() > 0 ? prev + curr.expectedTotal() : prev; }, 0);
    };
    VM.prototype.totalMeanExpence = function () {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce(function (prev, curr) { return curr.mean() <= 0 ? prev + curr.mean() : prev; }, 0);
    };
    VM.prototype.totalExpectedExpence = function () {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce(function (prev, curr) { return curr.expectedTotal() <= 0 ? prev + curr.expectedTotal() : prev; }, 0);
    };
    VM.prototype.remaining = function () {
        return this.totalIncome() + this.totalExpence();
    };
    return VM;
}());
//# sourceMappingURL=index.js.map