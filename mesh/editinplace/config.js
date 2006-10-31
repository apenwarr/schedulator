setWidgetsCustomConfig(
    function () {
        widgetsConfig.imgBasePath = '../configsystem/';
        widgetsConfig.editInPlace.cancelImgName = 'stop.gif';
        widgetsConfig.editInPlace.cancelImgWidth = 64;
        widgetsConfig.editInPlace.cancelImgHeight = 63;

        widgetsConfig.editInPlace.imageMap['useradminteam'] = {
            'user':'../configsystem/editinplace/icon_user.gif',
            'admin':'../configsystem/editinplace/icon_admin.gif',
            'team':'../configsystem/editinplace/icon_team.gif'
        };
    }
);
