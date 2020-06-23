The DailyDocuments ReadMe file


Introduction
============

DailyDocuments is an app to open documents, folders and even applications based on the date. Do you
need to open a spreadsheet every day to log your steps? Do you need to need to log your blood
pressure every Tuesday? Do you open a Word document every other day? How about opening your finance
software on the first day of the month? DailyDocuments can do this and more.


How DailyDocuments Works
========================

DailyDocuments reads a list of file names or applications from an XML file and displays them in a
scrollable window. The items in the list can be checked and opened together by clicking the Open Checked
button. Additionally, the list can contain scheduling information that will preselect items based on
the current date.

The File menu has options to Edit the XML file and to Reload the XML file. After changes have been
made to the XML file it will need to be reloaded or the application can be restarted. The format of
the XML file and the Day Codes for scheduling the list items are discussed in the following sections.

To the right of the menu items there is a day picker that can be used to change the date.



The XML File
============

When DailyDocuments starts it reads an XML file named DailyDocuments.xml that is located in the same
directory as DailyDocuments.exe.

This is a template for an entry in the file:

	<Entry>
		<Title></Title>
		<DocumentPath></DocumentPath>
		<Frequency>
			<Day></Day>
		</Frequency>
	</Entry>

	Place the text that you want to show in the DailyDocuments window between the Title tags.
	Place the complete path to the document, folder or application between the DocumentPath tags.
	Place the code for the days you want to have the item pre-checked between the Day tags. You
	can specify multiple days by having multiple sets of Day tags between a set of Frequency tags.
	For example the following will add "Test" which is found at C:\users\Tim\Documents\Important.doc,
	and select it on Tuesdays and Fridays.

	<Entry>
		<Title>Test</Title>
		<DocumentPath>C:\users\Tim\Documents\Important.doc</DocumentPath>
		<Frequency>
			<Day>TU</Day>
			<Day>FR</Day>
		</Frequency>
	</Entry>



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

M + 1 or 2 digits = Day of the month. Leading zeros are okay, but not required. For example, M12
for the 12th day of the month.

ML = Last day of the month.

Y + 1 to 3 digits = Day of the year. Leading zeros are okay, but not required. For example, Y100
for the 100th day of the year.

EVEN = Any day of the month that is evenly divisible by 2.
ODD = Days that are not considered even.

And finally, any specific date expressed as mm/dd where mm is 1 or 2 digits representing the
month and dd is 1 or 2 digits representing the day. Leading zeros are okay, but not required.
The separator can be either the "/" or the "-" character (without the quotes). For example, 11/25
for the 25th of November.

Note that there isn't any error checking for the day codes. If you specify M45 or Y367, that item
simply won't be selected.


Preferences
===========

You can choose to have the DailyDocuments stay on top of other windows.

You can choose to show file type icons.

You can choose to have the DailyDocuments window close after it opens the documents that are selected.

YOu can select the amount of delay between opening successive documents.

You can choose from three font sizes.

Note that settings are saved when the application is closed.


Automating Daily Documents
==========================

You can automate DailyDocuments by adding a task to Windows Task Scheduler, specifying the path to the
DailyDocuments executable in the Program field and /automatic in the Arguments field.



Notices and License
===================

DailyDocuments was written in C# by Tim Kennedy. Graphics & sound files were created by Tim Kennedy.


MIT License
Copyright (c) 2020 Tim Kennedy

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