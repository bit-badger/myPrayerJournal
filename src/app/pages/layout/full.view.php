<!DOCTYPE html>
<html lang="en">
<?php echo app()->template->render('layout/_head', [ 'pageTitle' => $pageTitle, 'isHtmx' => $isHtmx ]); ?>
<body>
    <section id="top" aria-label="Top navigation">
        <?php echo app()->template->render('layout/_nav', [ 'user' => $user, 'hasSnoozed' => $hasSnoozed ]); ?>
        <main role="main"><?php echo $pageContent; ?></main>
    </section>
    <?php echo app()->template->render('layout/_foot', [ 'isHtmx' => $isHtmx ]); ?>
</body>
</html>
