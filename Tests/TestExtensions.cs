namespace Tests;

public static class TestExtensions {

    public static ZonedDateTime ZoneDateTime(DateTimeZone zone, int year, int month, int day, int hour, int minute, int second = 0, int millisecond = 0) {
        return zone.AtStrictly(new LocalDateTime(year, month, day, hour, minute, second, millisecond));
    }

}