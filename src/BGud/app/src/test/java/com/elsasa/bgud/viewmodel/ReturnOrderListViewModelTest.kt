package com.elsasa.bgud.viewmodel

import com.elsasa.bgud.model.ReturnOrderEntity
import org.junit.Assert.assertEquals
import org.junit.Assert.assertTrue
import org.junit.Test
import java.util.Calendar

class ReturnOrderListViewModelTest {

    private fun createCalendar(year: Int, month: Int, day: Int, hour: Int = 12, minute: Int = 0): Calendar {
        return Calendar.getInstance().apply {
            set(Calendar.YEAR, year)
            set(Calendar.MONTH, month)
            set(Calendar.DAY_OF_MONTH, day)
            set(Calendar.HOUR_OF_DAY, hour)
            set(Calendar.MINUTE, minute)
            set(Calendar.SECOND, 0)
            set(Calendar.MILLISECOND, 0)
        }
    }

    @Test
    fun testDateSectionEnumValues() {
        assertEquals("TODAY", ReturnOrderDateSection.TODAY.displayName)
        assertEquals("YESTERDAY", ReturnOrderDateSection.YESTERDAY.displayName)
        assertEquals("EARLIER", ReturnOrderDateSection.EARLIER.displayName)
    }

    @Test
    fun testCategorizeDate_sameDay() {
        val now = createCalendar(2026, Calendar.SEPTEMBER, 25, 14, 30).timeInMillis
        val morningToday = createCalendar(2026, Calendar.SEPTEMBER, 25, 8, 0).timeInMillis
        val midnightStartToday = createCalendar(2026, Calendar.SEPTEMBER, 25, 0, 0).timeInMillis

        assertEquals(ReturnOrderDateSection.TODAY, ReturnOrderListViewModel.categorizeDate(morningToday, now))
        assertEquals(ReturnOrderDateSection.TODAY, ReturnOrderListViewModel.categorizeDate(midnightStartToday, now))
    }

    @Test
    fun testCategorizeDate_yesterday() {
        val now = createCalendar(2026, Calendar.SEPTEMBER, 25, 14, 30).timeInMillis
        val lateYesterday = createCalendar(2026, Calendar.SEPTEMBER, 24, 23, 59).timeInMillis
        val startYesterday = createCalendar(2026, Calendar.SEPTEMBER, 24, 0, 0).timeInMillis

        assertEquals(ReturnOrderDateSection.YESTERDAY, ReturnOrderListViewModel.categorizeDate(lateYesterday, now))
        assertEquals(ReturnOrderDateSection.YESTERDAY, ReturnOrderListViewModel.categorizeDate(startYesterday, now))
    }

    @Test
    fun testCategorizeDate_earlier() {
        val now = createCalendar(2026, Calendar.SEPTEMBER, 25, 14, 30).timeInMillis
        val twoDaysAgo = createCalendar(2026, Calendar.SEPTEMBER, 23, 23, 59).timeInMillis
        val lastMonth = createCalendar(2026, Calendar.AUGUST, 15, 10, 0).timeInMillis

        assertEquals(ReturnOrderDateSection.EARLIER, ReturnOrderListViewModel.categorizeDate(twoDaysAgo, now))
        assertEquals(ReturnOrderDateSection.EARLIER, ReturnOrderListViewModel.categorizeDate(lastMonth, now))
    }

    @Test
    fun testCategorizeDate_invalidOrZeroTimestamp() {
        val now = createCalendar(2026, Calendar.SEPTEMBER, 25, 14, 30).timeInMillis
        assertEquals(ReturnOrderDateSection.EARLIER, ReturnOrderListViewModel.categorizeDate(0L, now))
        assertEquals(ReturnOrderDateSection.EARLIER, ReturnOrderListViewModel.categorizeDate(-100L, now))
    }

    @Test
    fun testCategorizeDate_yearBoundary() {
        val newYearsDay = createCalendar(2026, Calendar.JANUARY, 1, 9, 0).timeInMillis
        val newYearsEve = createCalendar(2025, Calendar.DECEMBER, 31, 23, 0).timeInMillis
        val twoDaysBeforeNewYears = createCalendar(2025, Calendar.DECEMBER, 30, 15, 0).timeInMillis

        assertEquals(ReturnOrderDateSection.YESTERDAY, ReturnOrderListViewModel.categorizeDate(newYearsEve, newYearsDay))
        assertEquals(ReturnOrderDateSection.EARLIER, ReturnOrderListViewModel.categorizeDate(twoDaysBeforeNewYears, newYearsDay))
    }

    @Test
    fun testGroupByDateSection_emptyList() {
        val result = ReturnOrderListViewModel.groupByDateSection(emptyList())
        assertTrue(result.isEmpty())
    }

    @Test
    fun testGroupByDateSection_groupsAndPreservesOrder() {
        val now = createCalendar(2026, Calendar.SEPTEMBER, 25, 12, 0).timeInMillis

        val todayOrder1 = ReturnOrderListItem(
            order = ReturnOrderEntity(
                returnOrderId = "RO-1",
                customerName = "Customer A",
                createdAt = createCalendar(2026, Calendar.SEPTEMBER, 25, 10, 0).timeInMillis
            ),
            itemCount = 2
        )
        val todayOrder2 = ReturnOrderListItem(
            order = ReturnOrderEntity(
                returnOrderId = "RO-2",
                customerName = "Customer B",
                createdAt = createCalendar(2026, Calendar.SEPTEMBER, 25, 8, 0).timeInMillis
            ),
            itemCount = 1
        )
        val yesterdayOrder = ReturnOrderListItem(
            order = ReturnOrderEntity(
                returnOrderId = "RO-3",
                customerName = "Customer C",
                createdAt = createCalendar(2026, Calendar.SEPTEMBER, 24, 16, 0).timeInMillis
            ),
            itemCount = 5
        )
        val earlierOrder = ReturnOrderListItem(
            order = ReturnOrderEntity(
                returnOrderId = "RO-4",
                customerName = "Customer D",
                createdAt = createCalendar(2026, Calendar.SEPTEMBER, 20, 11, 0).timeInMillis
            ),
            itemCount = 3
        )

        val items = listOf(todayOrder1, todayOrder2, yesterdayOrder, earlierOrder)
        val grouped = ReturnOrderListViewModel.groupByDateSection(items, now)

        assertEquals(3, grouped.size)

        assertEquals(ReturnOrderDateSection.TODAY, grouped[0].section)
        assertEquals(listOf(todayOrder1, todayOrder2), grouped[0].items)

        assertEquals(ReturnOrderDateSection.YESTERDAY, grouped[1].section)
        assertEquals(listOf(yesterdayOrder), grouped[1].items)

        assertEquals(ReturnOrderDateSection.EARLIER, grouped[2].section)
        assertEquals(listOf(earlierOrder), grouped[2].items)
    }

    @Test
    fun testGroupByDateSection_omitsEmptySections() {
        val now = createCalendar(2026, Calendar.SEPTEMBER, 25, 12, 0).timeInMillis

        val yesterdayOrder = ReturnOrderListItem(
            order = ReturnOrderEntity(
                returnOrderId = "RO-1",
                customerName = "Customer Y",
                createdAt = createCalendar(2026, Calendar.SEPTEMBER, 24, 15, 0).timeInMillis
            ),
            itemCount = 1
        )
        val earlierOrder = ReturnOrderListItem(
            order = ReturnOrderEntity(
                returnOrderId = "RO-2",
                customerName = "Customer E",
                createdAt = createCalendar(2026, Calendar.SEPTEMBER, 22, 10, 0).timeInMillis
            ),
            itemCount = 2
        )

        // No items for TODAY
        val items = listOf(yesterdayOrder, earlierOrder)
        val grouped = ReturnOrderListViewModel.groupByDateSection(items, now)

        assertEquals(2, grouped.size)
        assertEquals(ReturnOrderDateSection.YESTERDAY, grouped[0].section)
        assertEquals(ReturnOrderDateSection.EARLIER, grouped[1].section)
    }
}
