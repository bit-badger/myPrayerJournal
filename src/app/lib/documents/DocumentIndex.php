<?php
declare(strict_types=1);

namespace BitBadger\PgDocuments;

/** The type of index to generate for the document */
enum DocumentIndex
{
    /** A GIN index with standard operations (all operators supported) */
    case Full;

    /** A GIN index with JSONPath operations (optimized for `@>`, `@?`, `@@` operators) */
    case Optimized;
}
