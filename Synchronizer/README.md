
 				How to use Synchronizer?

If you are as lazy as me am I you can build Synchronizer and copy into FoxLife\Recources


It just check matches node keys in lang.xaml and lang.ru-RU.xaml, rewrites file and add some nodes if it needed
(or with any another file, that you specify on Synchronizer. Or you can just create loop and do it all the files)


Advantages:

- Save few second to find where put lines in another localization
- Copies lines(nodes) from main file including innertext, so there is no space line in app
- Saves you own comments in another files


Disdvantages:

- Doesn't save spaces and enters(because it just rewrites file)
- Not sure about stability, because i doesn't test all variants(lazy :D)
- Need to add console menu to better experience
- Copies comments from main file to anothers


and ALWAYS do backup) 