<!DOCTYPE html>
<html lang="en">
<head>
    <title><?php echo $pageTitle; ?> &#xab; myPrayerJournal</title>
</head>
<body>
    <section id="top" aria-label="Top navigation">
        <?php echo app()->template->render('layout/_nav', [ 'user' => $user, 'hasSnoozed' => $hasSnoozed ]); ?>
        <main role="main"><?php echo $pageContent; ?></main>
    </section>
</body>
</html>
