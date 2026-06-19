using TimeZoneConverter;

namespace studrada_bot.Infrastructure;

public static class KyivTime
{
    public static readonly TimeZoneInfo Tz = TZConvert.GetTimeZoneInfo("Europe/Kyiv");

    public static DateTimeOffset Now() => TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow, Tz);

    public static DateTimeOffset ToKyiv(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, Tz);

    // GetUtcOffset враховує перехід літо/зима для конкретної дати
    public static DateTimeOffset FromKyiv(DateTime kyivDateTime) =>
        new DateTimeOffset(kyivDateTime, Tz.GetUtcOffset(kyivDateTime));
}