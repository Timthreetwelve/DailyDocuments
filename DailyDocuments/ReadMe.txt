The DailyDocuments ReadMe file


Introduction
============

DailyDocuments is an app to open documents, folders, applications or web sites based on the date.
Do you need to open a spreadsheet every day to log your steps? Do you need to need to log your blood
pressure every Tuesday? Do you open a Word document every other day? How about opening your personal
finance software on the first day of the month? DailyDocuments can do this and more.


How DailyDocuments Works
========================

DailyDocuments reads a list of file names or applications from a file and displays them in a scrollable
window. The items in the list can be checked and opened together by clicking the Open Checked button.
Additionally, the list can contain scheduling information that will preselect items based on the
current date.

When you first run DailyDocuments you will see a window with a list of examples that are meant to
demonstrate the different files, folders applications and web sites that can be opened by DailyDocuments.
These examples also show the scheduling options. After you are done investigating them, feel free to
delete them.


The Menu
========

The File menu has selections for List Maintenance, Preferences and to Exit the application. The Help
menu has selections to view this ReadMe file, to get help with Day Codes and an Advanced menu that
has selections that may be helpful in the event that troubleshooting is needed. To the right of the
menu items there is a day picker that can be used to change the date.


List Maintenance
================

To Add, delete or update list items, select the List Maintenance option from the File menu. This
will open the List Maintenance window. On the left side of the List Maintenance window you will
see a scrollable list. This list represents the items in the main window. When you click on an
item in the list, the text boxes on the right side of the window are populated with the details
of that list item. To update an item, simply make any changes in the appropriate text box and then
press the tab key or the enter key.

To arrange list items, simply drag an entry to a new position in the list.

To delete an item from the list, highlight that item in the list on the left side of the window and
then click the Delete Item button.

To add a new item, click the new item button. An new entry, temporarily named "untitled", is added
to the bottom of the list on the list. The text boxes on the right will have some instructional
text. That instructional text will be removed when you select that text box.

The Title text box is where you will enter the text that you want to see in the list in the main
window. It can say anything you want, perhaps Aunt Sally's Secret Buttermilk Biscuit Recipe.

The Path to Document text box is where you will enter the complete path to the file, folder
application, or website URL that you wish to open. For documents, folders and applications, you
may use environmental variables in the path. For example %temp% for the temp folder. For website
URLs, begin the URL with either https:\\ or http:\\.

The Day Codes text box is where you will enter the codes that will determine on which days the
items will be selected. The next section describes the individual day codes. This text box will
only accept alphanumeric, space, comma, dash (-) and forward slash (/) characters. It will also
attempt to insert commas where needed. All alphabetic characters will be converted to uppercase.

At the top right, above the text boxes, is an indicator that shows either "D2" or "DA". As you
will see in the next section, D2 and DA are the day codes for "every two days". This indicator
shows if the current day is a D2 day or a DA day, saving you from counting days from 1/1/2019.
There are a couple of key things to know about day codes, 1. you don't have to use them and,
2. if you do, separate multiple day codes with commas.

When you have finished updating the list, click the Save & Close button. There are also options on
the File menu to save without closing, save and close and to close without saving.


Day Codes
=========

D1 = Every day
D2 = Every two days, based on January 1st, 2019
DA = Every two days, based on January 2nd, 2019

SU = Sunday
MO = Monday
TU = Tuesday
WE = Wednesday
TH = Thursday
FR = Friday
SA = Saturday

WKD = Weekdays (Monday through Friday)
WKE = Weekends (Saturday and Sunday)

M + 1 or 2 digits = Day of the month. Leading zeros are okay, but not required. For example, M12
for the 12th day of the month.

ML = Last day of the month.

Y + 1 to 3 digits = Day of the year. Leading zeros are okay, but not required. For example, Y100
for the 100th day of the year.

EVEN = Any day of the month that is evenly divisible by 2.
ODD = Days that are not considered EVEN.

And finally, any specific date expressed as mm/dd where mm is 1 or 2 digits representing the month
and dd is 1 or 2 digits representing the day. Leading zeros are okay, but not required. The
separator can be either the "/" or the "-" character (without the quotes). For example, 02/06 or
2/6 for the 6th of February.

Some notes on day codes.

1. Day codes aren't required. If you just want items on the list but don't want them preselected,
then don't use a day code.

2. There isn't any error checking for the day codes. If you specify M45 or BR549, that item simply
won't be selected.


Preferences
===========

You can choose to have the DailyDocuments stay on top of other windows.

You can choose to show or not show file type icons.

You can choose to have the DailyDocuments window close after it opens the list items that are selected.

You can select the amount of delay between opening successive documents. Choosing a longer time may
be helpful on slower computers.

You can choose from three font sizes.

You can pick a background color for the list in the main window.

Note that preferences, along with widow size and position, are saved when the application is closed.


Automating DailyDocuments
==========================

You can have DailyDocuments start on a schedule by adding a task to Windows Task Scheduler, specifying
the path to the DailyDocuments executable in the Program field. For your convenience, there is an
option on the Help/Advanced menu to copy the path of the executable to the Windows clipboard.

If you want to go one step further, you can add /automatic as an argument and DailyDocuments will
open all of the items pre-selected to run that day.


Notices and License
===================

DailyDocuments was written in C# by Tim Kennedy.

DailyDocuments uses the following icons & packages:

Fugue Icons set https://p.yusukekamiyamane.com/

Json.net v12.0.3 from Newtonsoft https://www.newtonsoft.com/json

NLog v4.7.4 https://nlog-project.org/

Extended WPF Toolkit v3.8.1 from Xceed Software Inc. https://github.com/xceedsoftware/wpftoolkit

GongSolutions.WPF.DragDrop v2.2.0 https://github.com/punker76/gong-wpf-dragdrop


MIT License
Copyright (c) 2021 Tim Kennedy

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and
associated documentation files (the "Software"), to deal in the Software without restriction, including
without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject
to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial
portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT
LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.