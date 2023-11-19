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
            self::LessThanXMinutes => $singular ? 'less than a minute' : 'less than %i minutes',
            self::XMinutes         => $singular ? 'a minute'           : '%i minutes',
            self::AboutXHours      => $singular ? 'about an hour'      : 'about %i hours',
            self::XHours           => $singular ? 'an hour'            : '%i hours',
            self::XDays            => $singular ? 'a day'              : '%i days',
            self::AboutXWeeks      => $singular ? 'about a week'       : 'about %i weeks',
            self::XWeeks           => $singular ? 'a week'             : '%i weeks',
            self::AboutXMonths     => $singular ? 'about a month'      : 'about %i months',
            self::XMonths          => $singular ? 'a month'            : '%i months',
            self::AboutXYears      => $singular ? 'about a year'       : 'about %i years',
            self::XYears           => $singular ? 'a year'             : '%i years',
            self::OverXYears       => $singular ? 'over a year'        : 'over %i years',
            self::AlmostXYears     => $singular ? 'almost a year'      : 'almost %i years',
        };
    }
}
