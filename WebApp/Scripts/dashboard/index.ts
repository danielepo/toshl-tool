/// <reference path="../typings/knockout/knockout.d.ts" />
ko.options.deferUpdates = true;

function toCurrency(value: number): string {
    return value.toFixed(2).replace(/(\d)(?=(\d{3})+\.)/g, '$1,');
}
var Value = function (val) {
    this.number = ko.observable(Math.round(val));
}
interface KnockoutBindingHandlers {
    custValue: KnockoutBindingHandler;
}

var data1 = [];
var area1 = [];


interface IViewModelRow {
    entries: Array<number>;
    mean: number;
}
interface ITagValues {
    tag: string;
    row: IViewModelRow;
}
interface IEntry{
    value: number
}
class Entry implements IEntry{
    value: number;
    constructor(v: number) {
        this.value = v;
    }
}
interface IKnockoutViewModelRow {
    //entries: KnockoutObservableArray<KnockoutObservable<number>>;
    //entries: KnockoutObservableArray<number>;
    entries: Array<KnockoutObservable<IEntry>>;
    tag: string;
    mean: number;
    total: number;
    meanCurr: string;
    totalCurr: string;
    entryType: string;
}
var getEntry = (tagValues: ITagValues): IKnockoutViewModelRow => {

    var entryList = tagValues.row;
    var total = entryList.entries.reduce((prev, curr) => (prev + curr), 0);
    return {
        entries: entryList.entries.map(x => ko.observable(new Entry(x))),
        //entries: ko.observableArray(entryList.entries),
        //entries: ko.observableArray(entryList.entries.map(x => ko.observable(x))),
        tag: tagValues.tag,
        mean: tagValues.row.mean,
        total: total,
        meanCurr: toCurrency(entryList.mean),
        totalCurr: toCurrency(total),
        entryType: total > 0 ? "success" : "danger"
    };
}

var VM = function (entries: Array<ITagValues>) {


    var self = this;


    self.labels = [
        "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep",
        "Oct", "Nov", "Dic"
    ];
    self.entries = entries.map(elm => getEntry(elm));


    var totalIncome = ko.computed(() => this.entries.reduce((prev, curr) => {
        if (curr.total > 0)
            return prev + curr.total;
        else
            return prev;
    },
        0));


    var totalExpence = ko.computed(() => this.entries.reduce((prev, curr) => {
        if (curr.total <= 0)
            return prev + curr.total;
        else
            return prev;
    },
        0));

    var totalMeanIncome = ko.computed(() => this.entries.reduce((prev, curr) => {
        if (curr.mean > 0)
            return prev + curr.mean;
        else
            return prev;
    },
        0));


   var totalMeanExpence = ko.computed(() => this.entries.reduce((prev, curr) => {
        if (curr.mean <= 0)
            return prev + curr.mean;
        else
            return prev;
    },
        0));
    var remaining = ko.computed(() => (totalIncome() + totalExpence()));

    self.totalIncomeCurr = ko.computed(() => toCurrency(totalIncome()));
    self.totalExpenceCurr = ko.computed(() => toCurrency(totalExpence()));
    self.totalMeanIncomeCurr = ko.computed(() => toCurrency(totalMeanIncome()));
   self.totalMeanExpenceCurr = ko.computed(() => toCurrency(totalMeanExpence()));


    self.remainingCurr = ko.computed(() => toCurrency(remaining()));
    self.profitStatus = ko.computed(() => (remaining() > 0 ? "success" : "danger"));


    self.Data = {
        labels: self.labels,
        datasets: self.dataset1
    };


} 