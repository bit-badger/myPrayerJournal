<?php
declare(strict_types=1);

namespace MyPrayerJournal;

use DateTimeImmutable, DateTimeInterface, DateTimeZone;

class Dates
{
    /** Minutes in a day */
    private const A_DAY =  1_440;

    /** Minutes in two days(-ish) */
    private const ALMOST_2_DAYS = 2_520;
    
    /** Minutes in a month */
    private const A_MONTH = 43_200;
    
    /** Minutes in two months */
    private const TWO_MONTHS = 86_400;

    /**
     * Get a UTC-referenced current date/time
     * 
     * @return DateTimeImmutable The current date/time with UTC reference
     */
    public static function now(): DateTimeImmutable
    {
        return new DateTimeImmutable(timezone: new DateTimeZone('Etc/UTC'));
    }

    /**
     * Format the distance between two instants in approximate English terms
     * 
     * @param DateTimeInterface $startOn The starting date/time for the comparison
     * @param DateTimeInterface $endOn THe ending date/time for the comparison
     * @return string The formatted interval
     */
    public static function formatDistance(DateTimeInterface $startOn, DateTimeInterface $endOn): string
    {
        $diff    = $startOn->diff($endOn);
        $minutes =
            $diff->i + ($diff->h * 60) + ($diff->d * 60 * 24) + ($diff->m * 60 * 24 * 30) + ($diff->y * 60 * 24 * 365);
        $months = round($minutes / self::A_MONTH);
        $years  = $months / 12;
        [ $format, $number ] = match (true) {
            $minutes < 1                   => [ DistanceFormat::LessThanXMinutes, 1 ],
            $minutes < 45                  => [ DistanceFormat::XMinutes, $minutes ],
            $minutes < 90                  => [ DistanceFormat::AboutXHours, 1 ],
            $minutes < self::A_DAY         => [ DistanceFormat::AboutXHours, round($minutes / 60) ],
            $minutes < self::ALMOST_2_DAYS => [ DistanceFormat::XDays, 1 ],
            $minutes < self::A_MONTH       => [ DistanceFormat::XDays, round($minutes / self::A_DAY) ],
            $minutes < self::TWO_MONTHS    => [ DistanceFormat::AboutXMonths, round($minutes / self::A_MONTH) ],
            $months      < 12              => [ DistanceFormat::XMonths, round($minutes / self::A_MONTH) ],
            $months % 12 < 3               => [ DistanceFormat::AboutXYears, $years ],
            $months % 12 < 9               => [ DistanceFormat::OverXYears, $years ],
            default                        => [ DistanceFormat::AlmostXYears, $years + 1 ],
        };

        $relativeWords = sprintf(DistanceFormat::format($format, $number == 1), $number);
        return $startOn > $endOn ? "$relativeWords ago" : "in $relativeWords";
    }
}
