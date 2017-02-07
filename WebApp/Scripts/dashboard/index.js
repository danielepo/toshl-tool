/// <reference path="../typings/knockout/knockout.d.ts" />
function toCurrency(value) {
    return value.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}
var data1 = [];
var area1 = [];
var Value = function (val) {
    this.number = ko.observable(Math.round(val));
};
var Entry = function (tag, values) {
    var self = this;
    this.tag = tag;
    this.entries = values;
    this.total = ko.computed(function () {
        return values.reduce(function (prev, curr) { return prev + parseFloat(curr.number()); }, 0);
    });
    this.totalCurr = ko.computed(function () {
        return toCurrency(self.total());
    });
    self.entryType = ko.computed(function () {
        return self.total() > 0 ? "success" : "danger";
    });
};
var VM = function (entries) {
    var self = this;
    self.labels = [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep",
        "Oct", "Nov", "Dic"
    ];
    self.entries = entries.sort(function (a, b) { return a.total() < b.total() ? -1 : (a.total() > b.total() ? 1 : 0); });
    self.toalIncome = ko.computed(function () {
        return self.entries.reduce(function (prev, curr) {
            if (curr.total() > 0)
                return prev + curr.total();
            else
                return prev;
        }, 0);
    });
    self.totalIncomeCurr = ko.computed(function () {
        return toCurrency(self.toalIncome());
    });
    self.toalExpence = ko.computed(function () {
        return self.entries.reduce(function (prev, curr) {
            if (curr.total() <= 0)
                return prev + curr.total();
            else
                return prev;
        }, 0);
    });
    self.totalExpenceCurr = ko.computed(function () {
        return toCurrency(self.toalExpence());
    });
    self.remaining = ko.computed(function () {
        return self.toalIncome() + self.toalExpence();
    });
    self.remainingCurr = ko.computed(function () {
        return toCurrency(self.remaining());
    });
    self.profitStatus = ko.computed(function () {
        return self.remaining() > 0 ? "success" : "danger";
    });
    var colors = [];
    var objectGenerator = function (arr, tag) {
        var red;
        var green;
        var blue;
        if (typeof colors[tag] === "undefined") {
            red = Math.floor(Math.random() * 256);
            green = Math.floor(Math.random() * 256);
            blue = Math.floor(Math.random() * 256);
            colors[tag] = [red, green, blue];
        }
        else {
            red = colors[tag][0];
            green = colors[tag][1];
            blue = colors[tag][2];
        }
        return {
            label: tag,
            backgroundColor: "rgba(" + red + "," + green + "," + blue + ",0.2)",
            fill: false,
            lineTension: 0.1,
            borderJoinStyle: 'miter',
            borderColor: "rgba(" + red + "," + green + "," + blue + ",1)",
            pointColor: "rgba(" + red + "," + green + "," + blue + ",1)",
            pointStrokeColor: "#fff",
            pointHighlightFill: "#fff",
            pointHighlightStroke: "rgba(" + red + "," + green + "," + blue + ",1)",
            data: arr
        };
    };
    self.dataset1 = ko.computed(function () {
        var object = [];
        for (var j = 0; j < self.entries.length; j++) {
            var arr = [];
            for (var i = 0; i < 12; i++) {
                var val = parseFloat(self.entries[j].entries[i].number());
                if (val >= 0)
                    val = 0;
                arr.push(-val);
            }
            object.push(objectGenerator(arr, self.entries[j].tag));
        }
        return object;
    });
    self.dataset2 = ko.computed(function () {
        var object = [];
        for (var j = 0; j < self.entries.length; j++) {
            var arr = [];
            for (var i = 0; i < 12; i++) {
                var val = parseFloat(self.entries[j].entries[i].number());
                if (val >= 0)
                    val = 0;
                if (i === 0)
                    arr.push(-val);
                else
                    arr.push(-val + arr[i - 1]);
            }
            object.push(objectGenerator(arr, self.entries[j].tag));
        }
        return object;
    }).extend({ notify: 'always' });
    self.Data = {
        labels: self.labels,
        datasets: self.dataset1
    };
    self.Area = {
        labels: self.labels,
        datasets: self.dataset2
    };
};
