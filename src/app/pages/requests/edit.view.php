<?php

use MyPrayerJournal\Domain\RecurrenceType;

$cancelLink = match ($returnTo) {
    'active'  => '/requests/active',
    'snoozed' => '/requests/snoozed',
    default   => '/journal',
};
$isImmediate = $request->recurrenceType == RecurrenceType::Immediate;
$isHours     = $request->recurrenceType == RecurrenceType::Hours;
$isDays      = $request->recurrenceType == RecurrenceType::Days;
$isWeeks     = $request->recurrenceType == RecurrenceType::Weeks; ?>
<article class="container">
    <h2 class="pb-3"><?php echo $isNew ? 'Add' : 'Edit'; ?> Prayer Request</h2>
    <form hx-boost="true" hx-target="#top" hx-push-url="true"
          <?php echo $isNew ? 'hx-post' : 'hx-patch'; ?>="/request">
        <input type="hidden" name="requestId" value="<?php echo $isNew ? 'new' : $request->id; ?>">
        <input type="hidden" name="returnTo" value="<?php echo $returnTo; ?>">
        <div class="form-floating pb-3">
            <textarea id="requestText" name="requestText" class="form-control" style="min-height: 8rem;"
                      placeholder="Enter the text of the request"
                      autofocus required><?php echo $request->text; ?></textarea>
            <label for="requestText">Prayer Request</label>
        </div><br><?php
        if (!$isNew) { ?>
            <div class="pb-3">
                <label>Also Mark As</label><br>
                <div class="form-check form-check-inline">
                    <input type="radio" class="form-check-input" id="sU" name="status" value="Updated" checked>
                    <label for="sU">Updated</label>
                </div>
                <div class="form-check form-check-inline">
                    <input type="radio" class="form-check-input" id="sP" name="status" value="Prayed">
                    <label for="sP">Prayed</label>
                </div>
                <div class="form-check form-check-inline">
                    <input type="radio" class="form-check-input" id="sA" name="status" value="Answered">
                    <label for="sA">Answered</label>
                </div>
            </div><?php
        } ?>
        <div class="row">
            <div class="col-12 offset-md-2 col-md-8 offset-lg-3 col-lg-6">
                <p>
                    <strong>Recurrence</strong> &nbsp;
                    <em class="text-muted">After prayer, request reappears&hellip;</em>
                </p>
                <div class="d-flex flex-row flex-wrap justify-content-center align-items-center">
                    <div class="form-check mx-2">
                        <input type="radio" class="form-check-input" id="rI" name="recurType" value="Immediate"
                               onclick="mpj.edit.toggleRecurrence(event)" <?php echo $isImmediate ? 'checked' : ''; ?>>
                        <label for="rI">Immediately</label>
                    </div>
                    <div class="form-check mx-2">
                        <input type="radio" class="form-check-input" id="rO" name="recurType" value="Other"
                               onclick="mpj.edit.toggleRecurrence(event)" <?php echo $isImmediate ? '' : 'checked'; ?>>
                        <label for="rO">Every&hellip;</label>
                    </div>
                    <div class="form-floating mx-2">
                        <input type="number" class="form-control" id="recurCount" name="recurCount" placeholder="0"
                               value="<?php echo $request->recurrence ?? 0; ?>" style="width:6rem;" required
                               <?php echo $isImmediate ? 'disabled' : ''; ?>>
                        <label for="recurCount">Count</label>
                    </div>
                    <div class="form-floating mx-2">
                        <select class="form-control" id="recurInterval" name="recurInterval" style="width:6rem;"
                                required <?php echo $isImmediate ? 'disabled' : ''; ?>>
                            <option value="Hours" <?php echo $isHours ? 'selected' : ''; ?>>hours</option>
                            <option value="Days"  <?php echo $isDays  ? 'selected' : ''; ?>>days</option>
                            <option value="Weeks" <?php echo $isWeeks ? 'selected' : ''; ?>>weeks</option>
                        </select>
                        <label for="recurInterval">Interval</label>
                    </div>
                </div>
            </div>
        </div>
        <div class="text-end pt-3">
            <button class="btn btn-primary me-2" type="submit"><span class="material-icons">save</span> Save</button>
            <a <?php $page_link($cancelLink); ?> class="btn btn-secondary ms-2">
                <span class="material-icons">arrow_back</span> Cancel
            </a>
        </div>
    </form>
</article>
