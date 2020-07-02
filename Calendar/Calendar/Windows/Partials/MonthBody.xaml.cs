﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Calendar.Controllers;
using Calendar.Models;

namespace Calendar.Windows.Partials
{
    /// <summary>
    /// Lógica de interacción para MonthBody.xaml
    /// </summary>
    public partial class MonthBody : UserControl
    {
        #region Constants
        private const int iterationIndexOffset = 1;
        private const int gridRowIndexOffset = 1;
        private const int gridColumnIndexOffset = 1;
        private const int firstDayNumberInMonth = 1;
        private const int numberOfCellsInGrid = 42;
        private const int saturdayGridColumnIndex = 5;
        private const int sundayGridColumnIndex = 6;
        #endregion


        #region Fields
        private readonly Brush highlightColor = Brushes.Red;
        private SessionController sourceSessionController;
        private List<MonthDayElement> dayElements = new List<MonthDayElement>();
        private List<Appointment> monthAppointmens;
        #endregion


        #region Properties
        #endregion


        #region Methods
        public MonthBody(SessionController sourceSessionController)
        {
            this.sourceSessionController = sourceSessionController;
            InitializeComponent();
        }

        public void Refresh() 
        {
            GenerateDayElements();
            InsertBodyElements();
            RefreshDayElements();
            HighLightWeekends();
        }

        public void AssignMonthAppointments(List<Appointment> appointments)
        {
            monthAppointmens = appointments;
        }

        private void GenerateDayElements()
        {
            dayElements = new List<MonthDayElement>();
            for (int i = 0; i < numberOfCellsInGrid; i++)
            {
                int candidateDayNumber = i - GetfirstDayGridColumnIndex() + iterationIndexOffset;
                Point dayElementGridCoordinates = GetGridCoordinatesByIterationIndex(i);
                bool isDayNumberInDisplayedMonth = IsDayNumberInDisplayedMonth(candidateDayNumber, dayElementGridCoordinates);

                if (isDayNumberInDisplayedMonth)
                {
                    int year = DateUtilities.DisplayedDate.Year;
                    int month = DateUtilities.DisplayedDate.Month;
                    int day = candidateDayNumber;
                    DateTime dayElementDate = new DateTime(year, month, day);

                    MonthDayElement monthDayElement = new MonthDayElement(dayElementDate, sourceSessionController);
                    monthDayElement.SetValue(Grid.ColumnProperty, (int)dayElementGridCoordinates.X);
                    monthDayElement.SetValue(Grid.RowProperty, (int)dayElementGridCoordinates.Y);

                    dayElements.Add(monthDayElement);
                }
                else 
                {
                    MonthDayElement monthDayElementBlank = new MonthDayElement();
                    monthDayElementBlank.SetValue(Grid.ColumnProperty, (int)dayElementGridCoordinates.X);
                    monthDayElementBlank.SetValue(Grid.RowProperty, (int)dayElementGridCoordinates.Y);

                    dayElements.Add(monthDayElementBlank);
                }
            }
        }

        private void InsertBodyElements() 
        {
            BodyGrid.Children.Clear();
            InsertWeekDaysToGrid();
            InsertDayElementsToGrid();
        }

        private void InsertWeekDaysToGrid() 
        {
            for (int i = 0; i < 7; i++)
            {
                Collection<string> dayNames = DateUtilities.DayNames;
                TextBlock textBlockDayName = new TextBlock
                {
                    Text = dayNames[i]
                };
                Border borderDayName = new Border();
                textBlockDayName.SetValue(Grid.ColumnProperty, i);
                borderDayName.SetValue(Grid.ColumnProperty, i);
                BodyGrid.Children.Add(textBlockDayName);
                BodyGrid.Children.Add(borderDayName);
            }
        }

        private void InsertDayElementsToGrid()
        {
            foreach (MonthDayElement dayElement in dayElements)
            {
                BodyGrid.Children.Add(dayElement);
            }
        }

        private void HighLightWeekends()
        {
            foreach (var child in BodyGrid.Children)
            {
                if (IsDayElement(child))
                {
                    MonthDayElement dayElementChild = child as MonthDayElement;
                    int childColumnIndex = (int)dayElementChild.GetValue(Grid.ColumnProperty);
                    if (IsInWeekendColumn(childColumnIndex))
                    {
                        dayElementChild.Foreground = highlightColor;
                    }
                }
                else if(IsWeekDayNameElement(child))
                {
                    TextBlock textBlockDayNameElement = child as TextBlock;
                    int childrenColumnIndex = (int)textBlockDayNameElement.GetValue(Grid.ColumnProperty);
                    if (IsInWeekendColumn(childrenColumnIndex))
                    {
                        textBlockDayNameElement.Foreground = highlightColor;
                    }
                }
            }
        }

        private void RefreshDayElements()
        {
            foreach (MonthDayElement dayElement in dayElements)
            {
                dayElement.AssignDayAppointments(GetDayAppointments(dayElement));
                dayElement.Refresh();
            }
        }

        private List<Appointment> GetDayAppointments(MonthDayElement dayElement)
        {
            List<Appointment> dayElementAppointments = new List<Appointment>();

            foreach (Appointment appointment in monthAppointmens)
            {
                bool hasReadPermission = appointment.HasReadPermissions(sourceSessionController.CurrentUsername);
                bool isOfTheDayElement = IsAppointmentOfDay(appointment, dayElement);

                if (isOfTheDayElement & hasReadPermission)
                {
                    dayElementAppointments.Add(appointment);
                }
            }

            return dayElementAppointments;
        }

        private static bool IsInWeekendColumn(int childrenColumnIndex)
        {
            bool isInSaturday = childrenColumnIndex == saturdayGridColumnIndex;
            bool isInSunday = childrenColumnIndex == sundayGridColumnIndex;
            bool isInWeekend = isInSaturday | isInSunday;
            return isInWeekend;
        }

        private static bool IsDayElement(object child)
        {
            bool isDayElement = child.GetType() == typeof(MonthDayElement);
            return isDayElement;
        }

        private static bool IsWeekDayNameElement(object children)
        {
            bool isTextBlockElement = children.GetType() == typeof(TextBlock);
            return isTextBlockElement;
        }

        private static bool IsDayNumberInDisplayedMonth(int candidateDayNumber, Point dayElementGridCoordinates)
        {
            const int firstDayRowIndex = 1;
            DateTime displayedDate = DateUtilities.DisplayedDate;
            bool isFirstDayRow = dayElementGridCoordinates.Y == firstDayRowIndex;
            bool isNotFirstDayRow = dayElementGridCoordinates.Y > firstDayRowIndex;
            bool isFirstDayColumnOrLater = dayElementGridCoordinates.X >= GetfirstDayGridColumnIndex();
            bool isDisplayableDayElementOfFirstRow = isFirstDayRow && isFirstDayColumnOrLater;
            bool isCandidateDayNumberInDisplayedMonth = candidateDayNumber <= GetNumberOfDaysOfMonth(displayedDate);
            bool isDisplayableDayElementOfRemainsRows = isNotFirstDayRow && isCandidateDayNumberInDisplayedMonth;
            return (isDisplayableDayElementOfFirstRow || isDisplayableDayElementOfRemainsRows);
        }

        private static bool IsAppointmentOfDay(Appointment appointment, MonthDayElement dayElement) 
        {
            bool areTheSameDate = appointment.Start.Date == dayElement.Date.Date;
            return areTheSameDate;
        }

        private static int GetNumberOfDaysOfMonth(DateTime date)
        {
            DateTime displayedDate = date;
            int numberOfDaysOfDisplayedMonth = DateTime.DaysInMonth(displayedDate.Year, displayedDate.Month);
            return numberOfDaysOfDisplayedMonth;
        }

        private static int GetfirstDayGridColumnIndex()
        {
            DateTime displayedDate = DateUtilities.DisplayedDate;
            DateTime firstDayOfDisplayedMonth = new DateTime(displayedDate.Year, displayedDate.Month, firstDayNumberInMonth);
            int firstDayGridColumnIndex = DateUtilities.GetDayNumberInWeek(firstDayOfDisplayedMonth) - gridColumnIndexOffset;
            return firstDayGridColumnIndex;
        }

        private static Point GetGridCoordinatesByIterationIndex(int iterationIndex)
        {
            int gridColumn = (iterationIndex) % DateUtilities.DaysInWeek;
            int gridRow = (iterationIndex / DateUtilities.DaysInWeek) + gridRowIndexOffset;
            return new Point(gridColumn, gridRow); ;
        }

        #endregion
    }
}
