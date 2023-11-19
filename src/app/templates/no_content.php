<div class="card mb-3">
    <div class="card-header has-background-light">
        <h5 class="card-header-title"><?php echo $_REQUEST['EMPTY_HEADING']; ?></h5>
    </div>
    <div class="card-content has-text-centered">
        <p class="mb-5"><?php echo $_REQUEST['EMPTY_TEXT']; ?></p>
        <a <?php page_link($_REQUEST['EMPTY_LINK']); ?>
           class="button is-link"><?php echo $_REQUEST['EMPTY_BTN_TXT']; ?></a>
    </div>
</div>
