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

var months: string[] = [
    "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep",
    "Oct", "Nov", "Dic"
];

interface IViewModelRow {
    entries: Array<number>;
}
interface ITagValues {
    tag: string;
    category: string;
    row: IViewModelRow;
}
interface IEntry {
    value: KnockoutObservable<string>;
    month: string;
}
class Entry implements IEntry {
    value: KnockoutObservable<string>;
    month: string;
    getValue = (): number => parseFloat(this.value());
    constructor(v: number, month: number) {
        this.value = ko.observable(v + "");
        this.month = months[month];
    }

}
interface IKnockoutViewModelRow {
    //entries: KnockoutObservableArray<KnockoutObservable<number>>;
    //entries: KnockoutObservableArray<number>;
    entries: Array<IEntry>;
    tag: string;
    category: string;
    mean: KnockoutObservable<number>;
    total: KnockoutObservable<number>;
    meanCurr: KnockoutObservable<string>;
    totalCurr: KnockoutObservable<string>;
    entryType: string;
    expectedTotal: KnockoutObservable<number>;
    getEntryType(row1: IKnockoutViewModelRow, id: KnockoutObservable<number>, vm: IDashboardVm) : string;
}
class KnockoutEntry implements IKnockoutViewModelRow {
    entries: Array<IEntry>;
    tag: string;
    category: string;
    mean: KnockoutObservable<number>;
    total: KnockoutObservable<number>;
    meanCurr: KnockoutObservable<string>;
    totalCurr: KnockoutObservable<string>;
    entryType: string;
    expectedTotal: KnockoutObservable<number>;
    constructor(tagValues: ITagValues) {

        var entryList = tagValues.row;
        var observableEntries = entryList.entries.map((x, i) => new Entry(x, i));
        var total = ko.pureComputed(() => observableEntries.reduce(function (prev, curr) {
            return (prev + curr.getValue());
        }, 0));
        var mean = ko.pureComputed(() => {
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
        this.meanCurr = ko.pureComputed(() => toCurrency(mean()));
        this.expectedTotal = ko.pureComputed(() => mean() * 12);
        this.totalCurr = ko.pureComputed(() => toCurrency(total()));
        this.entryType = total() > 0 ? "success" : "danger";
    }
    getEntryType(row1: IKnockoutViewModelRow, id: KnockoutObservable<number>, vm: IDashboardVm) {
        debugger;
        if (id() == 0)
            return row1.total() > 0 ? "success" : "danger";

        if (row1.category == vm.entries[id() - 1].category)
            return row1.total() > 0 ? "success" : "danger";
        else
            return row1.total() > 0 ? "success separator" : "danger separator";

    }

}

interface IDashboardVm {
    labels: string[];
    entries: IKnockoutViewModelRow[];
    totalIncomeCurr: KnockoutComputed<string>;
    totalExpenceCurr: KnockoutComputed<string>;
    totalMeanIncomeCurr: KnockoutComputed<string>;
    totalMeanExpenceCurr: KnockoutComputed<string>;
    remainingCurr: KnockoutComputed<string>;
    profitStatus: KnockoutComputed<string>;
}
class VM implements IDashboardVm {

    labels: string[];
    entries: IKnockoutViewModelRow[];
    constructor(entries: Array<ITagValues>) {
        this.labels = months;
        this.entries = entries.map(elm => new KnockoutEntry(elm));
    }

    private totalIncome() {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce((prev, curr) => curr.total() > 0 ? prev + curr.total() : prev, 0);
    }

    private totalExpence() {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce((prev, curr) => curr.total() <= 0 ? prev + curr.total() : prev, 0);
    }

    private totalMeanIncome() {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce((prev, curr) => curr.mean() > 0 ? prev + curr.mean() : prev, 0);
    }

    private totalExpectedIncome() {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce((prev, curr) => curr.expectedTotal() > 0 ? prev + curr.expectedTotal() : prev, 0);
    }
    private totalMeanExpence() {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce((prev, curr) => curr.mean() <= 0 ? prev + curr.mean() : prev, 0);
    }
    private totalExpectedExpence() {
        if (this.entries == null) {
            return 0;
        }
        return this.entries.reduce((prev, curr) => curr.expectedTotal() <= 0 ? prev + curr.expectedTotal() : prev, 0);
    }
    private remaining() {
        return this.totalIncome() + this.totalExpence();
    }
    totalIncomeCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.totalIncome()));
    totalExpenceCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.totalExpence()));
    totalMeanIncomeCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.totalMeanIncome()));
    totalExpectedIncomeCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.totalExpectedIncome()));
    totalMeanExpenceCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.totalMeanExpence()));
    totalExpectedExpenceCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.totalExpectedExpence()));
    remainingCurr: KnockoutComputed<string> = ko.pureComputed(() => toCurrency(this.remaining()));
    profitStatus: KnockoutComputed<string> = ko.pureComputed(() => (this.remaining() > 0 ? "success" : "danger"));
}
