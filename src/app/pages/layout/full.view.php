<!DOCTYPE html>
<html lang="en">
<?php echo app()->template->render('layout/_head', [ 'pageTitle' => $pageTitle, 'isHtmx' => $isHtmx ]); ?>
<body>
    <section id="top" aria-label="Top navigation">
        <?php echo app()->template->render('layout/_nav', [ 'userId' => $userId ]); ?>
        <?php echo $pageContent; ?>
    </section>
    <?php echo app()->template->render('layout/_foot', [ 'isHtmx' => $isHtmx ]); ?>
</body>
</html>
