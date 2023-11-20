<?php
declare(strict_types=1);

namespace MyPrayerJournal;

/**
 * The different distance formats supported
 */
enum DistanceFormat
{
    case LessThanXMinutes;
    case XMinutes;
    case AboutXHours;
    case XHours;
    case XDays;
    case AboutXWeeks;
    case XWeeks;
    case AboutXMonths;
    case XMonths;
    case AboutXYears;
    case XYears;
    case OverXYears;
    case AlmostXYears;

    /**
     * Return the formatting string for the given format and number
     * 
     * @param DistanceFormat $it The distance format
     * @param bool $singular If true, returns the singular version; if false (default), returns the plural version
     * @return string The format string
     */
    public static function format(DistanceFormat $it, bool $singular = false): string
    {
        return match ($it) {
            self::LessThanXMinutes => $singular ? 'less than a minute' : 'less than %d minutes',
            self::XMinutes         => $singular ? 'a minute'           : '%d minutes',
            self::AboutXHours      => $singular ? 'about an hour'      : 'about %d hours',
            self::XHours           => $singular ? 'an hour'            : '%d hours',
            self::XDays            => $singular ? 'a day'              : '%d days',
            self::AboutXWeeks      => $singular ? 'about a week'       : 'about %d weeks',
            self::XWeeks           => $singular ? 'a week'             : '%d weeks',
            self::AboutXMonths     => $singular ? 'about a month'      : 'about %d months',
            self::XMonths          => $singular ? 'a month'            : '%d months',
            self::AboutXYears      => $singular ? 'about a year'       : 'about %d years',
            self::XYears           => $singular ? 'a year'             : '%d years',
            self::OverXYears       => $singular ? 'over a year'        : 'over %d years',
            self::AlmostXYears     => $singular ? 'almost a year'      : 'almost %d years',
        };
    }
}
