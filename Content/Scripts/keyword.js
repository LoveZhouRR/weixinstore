(function ($) {
    Array.prototype.remove = function (idx) {
        if (idx < 0 || idx >= this.length) return this;
        return this.slice(0, n).concat(this.slice(n + 1, this.length));
    }
    var modal = function(options) {
        options = options || {};
        var target = $('<div class="modal fade"  data-keyboard="false" data-backdrop="static" tabindex="-1" role="dialog" aria-labelledby="myModalLabel" aria-hidden="true">' +
                '<div class="modal-dialog"><div class="modal-content">' +
                '<div class="modal-header">' +
                '<button type="button" class="close" data-dismiss="modal" aria-hidden="true">&times;</button>' +
                '<h4 class="modal-title" style="font-size:18px;font-weight:500;">'+ options.title + '</h4></div>' +
                '<div class="modal-body">'+ options.content + '</div>' +
            '<div class="modal-footer">' +
                '<a href="javascript:void(0)" name="save" class="btn btn-primary">保存</a>' + 
                '<button type="button" class="btn btn-default" data-dismiss="modal">取消</button></div></div></div></div>'
            ).appendTo('body');
        target.on('hidden.bs.modal', function () {
            target.remove();
        })
        target.on('show.bs.modal',function() {
            if (options.open) {
                options.open(target);
            }
        })
        target.find('a[name=save]').on('click', function(e) {
            options.save(target);
        });
        
        target.modal();
    }
    var Rule = function (containerElement, options) {
        this.options =
            this.$element =
                this.store =
                    this.editing =
                        this.template =
                            this.$container =
                                this.action =
                null;
        this.init(containerElement, options);
    };
    Rule.DEFAULTS = {
        shellTemplate: '<div class="ar_mn"></div>',
        showTemplate: '<div class="ar_mt"><b>规则：</b><em></em><a href="javascript:;">展开</a></div>' +
            '<div class="ar_mm"><dl class="ar_mdl"> <dt><span>关键字</span><div class="ar_mdlm"></div></dt><dd><span>回复</span>' +
            '<div class="ar_mdlm"></div></dd></dl></div>',
        editingTemplate: '<div class="ar_mt"><b>添加规则：</b><em></em><a href="javascript:;"></a></div>' +
            '<div class="ar_mm"><dl class="ar_unfold"><dt class="ar_re_t"><span><i class="icon"></i>规则名</span><input type="text" /><em>规则名最多60个字</em></dt><dd class="ar_kd_t"><span>' +
            '<i class="icon"></i>关键字</span> </dd>' +
            '<dd class="ar_kd_m"><input type="text" placeholder="回车添加" maxlength="10"></dd>' +
            '<dd class="ar_rt_t"><span><i class="icon"></i>回复</span></dd>' +
            '<dd class="ar_media_list">' +
            '<li><a title="添加文字回复" data-media-type="1"><i class="fa fa-file-text fa-2x"></i>&nbsp;文字</a></li>' +
            '<li><a title="添加图片回复" data-media-type="5"><i class="fa fa-picture-o fa-2x"></i>&nbsp;图片</a></li>' +
            '<li><a title="添加语音回复" data-media-type="3"><i class="fa fa-volume-up fa-2x"></i>&nbsp;语音</a></li>' +
            '<li><a title="添加视频回复" data-media-type="4"><i class="fa fa-video-camera fa-2x"></i>&nbsp;视频</a></li>' +
            //'<li><a title="添加图文回复" data-media-type="2"><i class="fa fa-comments-o"></i>&nbsp;图文</a></li>' +
            '</dd>' +
            '<dd class="ar_re_m"></dd>' +
            '<dd class="ar_ud_sub"><a href="javascript:;" name="save">保 存</a><a href="javascript:;" class="ar_ud_ss">删 除</a></dd></dl></div>'
    };
    Rule.prototype.init = function (element, options) {
        this.$container = $(element);
        this.options = this.getOptions(options);
        this.editing = false;
        var t = this.options.shellTemplate;
        if (this.action != 'create') {
            this.$element = $(t).appendTo(this.$container);
        } else {
            this.$element = $(t).prependTo(this.$container);
        }
        this.render()
    }
    var getImage = function(imgResource,type) {
        var content = []
        $.each(imgResource, function (i, m) {
            content.push('<li class="media_item">')
            content.push('<div class="media_info">')
            content.push('<label class="media_label"><input type="radio" name="imgRadio" value="' + m.ID + '" data-img-path="'+m.Path+'"/>')
            content.push(m.OriginName)
            content.push('</label>')
            content.push('<span class="media_size">'+m.Size+'byte</span>')
            content.push('</div>')
            content.push('<div class="media_content">')
            switch (type+'') {
            
                case '3'://音频
                    content.push('<audio controls="controls"><source src="'+m.Path+'" type="audio/mpeg"></audio>')
                    break;
                case '4'://视频
                    content.push('<video width="320" height="240" controls="controls"><source src="'+m.Path+'" type="video/mp4"></video>')
                    break;
                case '5'://图片
                    content.push('<img style="max-width:100px;max-height:70px;" src="' + m.Path + '"/>')
                    break;
            }
            content.push('</div>')
            content.push('</li>')
            
        })
        return content.join('')
    }
    Rule.prototype.render = function () {
        var me = this;
        if (this.action == 'create' || this.editing) {
            var $ele = this.$element;
            this.$element.html(this.options.editingTemplate);
            
            //保存
            this.$element.find('.ar_ud_sub a[name=save]').click(function() {
                var name = $ele.find('.ar_re_t input').val();
                if (!name) {
                    $.error('规则名不能为空');
                    return;
                }
                if (name.length > 60) {
                    $.error('规则名最多60个字');
                    return;
                }
                var keywords = me.getKeywords();
                if (keywords.length <= 0) {
                    $.error('请至少添加一个关键词');
                    return;
                }
                var res = me.getResource();
                if (!res || !res.length) {
                    $.error('请至少添加一条回复');
                    return;
                }
                var model = me.updateModel();
                
                me.$element.mask('保存中，请稍等');
                $.ajax({
                    type: 'POST',
                    url: '/keywordreply/Modify',
                    data: JSON.stringify({item:model}),
                    dataType: 'json',
                    contentType: "application/json",
                    success: function (msg) {
                        me.$element.unmask();
                        if (msg.success) {
                            window.location.reload();
                        } else {
                            $.error(msg.msg);
                        }
                    }
                })
            })
            if (this.action == 'create') {
                this.$element.find('.ar_ud_sub .ar_ud_ss').hide();
            } else {
                this.$element.find('.ar_ud_sub .ar_ud_ss').attr('href','/keywordreply/delete?ruleId='+me.getModel().ID)
            }
            //添加关键字
            this.$element.find('.ar_kd_m input[type=text]').bind('keypress',function(e) {
                var key = window.event ? e.keyCode : e.which;
                if (key == "13") {
                    var val = $(this).val();
                    if (!val) return;
                    me.addKeyword({ Content: val });
                    $(this).val('');
                }
            })
            //添加回复
            this.$element.find('.ar_media_list li a').click(function() {
                var target = $(this);
                var res = me.getResource();
                if (res && res.length >= 10) {
                    $.error('最多添加10条回复');
                    return;
                }
                var dataType = target.attr('data-media-type');
                var loadPage = function (t, page, url, type) {
                    type = type;
                    $.ajax({
                        method: 'GET',
                        url: url,
                        dataType: 'json',
                        success: function (msg) {
                            var c = getImage(msg.rows,type);
                            t.find('.media_list_dialog').html(c);
                            var pager = t.find('.media_page').data('pager');
                            pager.current = page;
                            pager.total = msg.total;
                            t.find('.media_page span').html('' + pager.current + '/' + (pager.total / pager.step + 1).toFixed(0))
                        }
                    })
                }
                switch (dataType + '') {
                //文字 
                    case '1':
                        modal({
                            title: '添加回复文字',
                            content: '<textarea style="width:100%;height:100px;resize:none;" placeholder="请添加回复文字，最多250个字符"></textarea>',
                            save:function(t) {
                                var text = t.find('textarea').val();
                                if (!text) {
                                    $.error('回复不能为空');
                                    return;
                                }
                                if (text.length > 250) {
                                    $.error('最多回复250个字符');
                                    return;
                                }
                                me.addResource({data:{Content:text},type:dataType})
                                t.modal('hide');
                            }
                        });
                        break;
                        
                    case '2'://图文
                        break;
                    case '3'://语音
                        modal({
                            title: '选择素材',
                            content: '<div><div class="media_page"><a name="prev">上一页</a><span>1/10</span><a name="next">下一页</a></div><div class="media_list_dialog"></div></div>',
                            save:function(t) {
                                var val = t.find('input[name=imgRadio]:checked').val();
                                if (!val) {
                                    $.error('没有选中任何素材');
                                    return;
                                }
                                var path = t.find('input[name=imgRadio]:checked').attr('data-img-path');
                                me.addResource({ data: { ResourceType: dataType, ResourceID: val, Path: path }, type: dataType });
                                t.modal('hide');
                            },
                            open:function(t) {
                                t.find('.media_page').data('pager', { current: 1, total: 10, step: 10 });
                                t.find('.media_page a[name=prev]').click(function () {
                                    var pager = t.find('.media_page').data('pager');
                                    if (pager.current > 1) {
                                        loadPage(t, pager.current - 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + (page.current - 1), dataType);
                                    }
                                })
                                t.find('.media_page a[name=next]').click(function () {
                                    var pager = t.find('.media_page').data('pager');
                                    if (pager.total / pager.step > pager.current) {
                                        loadPage(t, pager.current + 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + (page.current + 1), dataType);
                                    }
                                })
                                loadPage(t, 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + 1, dataType);
                            }
                        })
                        break;
                    case '4'://视频
                        modal({
                            title: '选择素材',
                            content: '<div><div class="media_page"><a name="prev">上一页</a><span>1/10</span><a name="next">下一页</a></div><div class="media_list_dialog"></div></div>',
                            save: function (t) {
                                var val = t.find('input[name=imgRadio]:checked').val();
                                if (!val) {
                                    $.error('没有选中任何素材');
                                    return;
                                }
                                var path = t.find('input[name=imgRadio]:checked').attr('data-img-path');
                                me.addResource({ data: { ResourceType: dataType, ResourceID: val, Path: path }, type: dataType });
                                t.modal('hide');
                            },
                            open: function (t) {
                                t.find('.media_page').data('pager', { current: 1, total: 10, step: 10 });
                                t.find('.media_page a[name=prev]').click(function () {
                                    var pager = t.find('.media_page').data('pager');
                                    if (pager.current > 1) {
                                        loadPage(t, pager.current - 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + (page.current - 1), dataType);
                                    }
                                })
                                t.find('.media_page a[name=next]').click(function () {
                                    var pager = t.find('.media_page').data('pager');
                                    if (pager.total / pager.step > pager.current) {
                                        loadPage(t, pager.current + 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + (page.current + 1), dataType);
                                    }
                                })
                                loadPage(t, 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + 1, dataType);
                            }
                        })
                        break;
                    case '5'://图片
                        modal({
                            title: '选择素材',
                            content: '<div><div class="media_page"><a name="prev">上一页</a><span>1/10</span><a name="next">下一页</a></div><div class="media_list_dialog"></div></div>',
                            save: function (t) {
                                var val = t.find('input[name=imgRadio]:checked').val();
                                if (!val) {
                                    $.error('没有选中任何素材');
                                    return;
                                }
                                var path = t.find('input[name=imgRadio]:checked').attr('data-img-path');
                                me.addResource({ data: { ResourceType: dataType, ResourceID: val, Path: path }, type: dataType });
                                t.modal('hide');
                            },
                            open: function (t) {
                                t.find('.media_page').data('pager', { current: 1, total: 10, step: 10 });
                                t.find('.media_page a[name=prev]').click(function() {
                                    var pager = t.find('.media_page').data('pager');
                                    if (pager.current > 1) {
                                        loadPage(t, pager.current - 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + (page.current - 1), dataType);
                                    }
                                })
                                t.find('.media_page a[name=next]').click(function() {
                                    var pager = t.find('.media_page').data('pager');
                                    if (pager.total / pager.step > pager.current) {
                                        loadPage(t, pager.current + 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + (page.current + 1), dataType);
                                    }
                                })
                                loadPage(t, 1, '/keywordreply/GetResource?type=' + dataType + '&page=' + 1, dataType);
                            }
                        })
                        
                        break;
                }
            })
        } else {
            this.$element.html(this.options.showTemplate);
        }
        
        var m = this.getModel();
        if (m) {
            
            me.$element.find('.ar_mt a').click(function() {
                me.toggleEdit();
            })
            if (this.editing) {
                me.$element.find('.ar_mt a').html('收起');
                me.setTitleHint('规则：')
            }
        }
        if (this.action != 'create' && m) {
            var $e = this.$element;
            $e.find('.ar_mt em').html(m.Name);
            if (this.editing) {
                $.each(m.Keywords, function(i, t) {
                    //$e.find('.ar_kd_m').append('<a><em>' + t.Content + '</em><i>x</i></a>');
                    me.addKeyword(t);
                })
                $e.find('.ar_re_t input').val(m.Name);
                if (m.TextReply) {
                    $.each(m.TextReply, function(i, t) {
                        me.addResource({ data: t, type: '1' });
                    })
                }
                if (m.ResourceItems) {
                    $.each(m.ResourceItems, function(i, t) {
                        me.addResource({ data: t, type: t.ResourceType});
                    })
                }
            } else {
                $.each(m.Keywords, function (i, t) {
                    $e.find('.ar_mdl dt div').append('<em>' + t.Content + '</em>');
                })
                var textreplies = m.TextReply || [];
                var resources = m.ResourceItems || [];
                var total = 0;
                total = textreplies.length + resources.length;
                var pic = 0, audio = 0, video = 0, news = 0;
                for (var idx = 0; idx < resources.length; idx = idx + 1) {
                    var r = resources[idx]
                    if (r) {
                        switch (r.ResourceType) {
                            case 2:
                                news = news + 1;
                                break;
                            case 3:
                                audio = audio + 1;
                                break;
                            case 4:
                                video = video + 1;
                                break;
                            case 5:
                                pic = pic + 1;
                                break;
                        }
                    }
                }
                var out = $.format('{0}条（{1}条文字，{2}条图片, {3}条语音, {4}条视频, {5}条图文）',
                    total,textreplies.length,pic,audio, video,news
                )
                $e.find('.ar_mdl dd div').html(out);
            }
        }
    }
    Rule.prototype.getDefaults = function () {
        return Rule.DEFAULTS;
    }
    Rule.prototype.getOptions = function (options) {
        options = $.extend({}, this.getDefaults(), this.$container.data(), options)
        if (!options.model) {
            this.action = 'create'
        }
        return options
    }


    Rule.prototype.setTitleHint = function (hint) {
        this.$element.find('.ar_mt b').html(hint)
    }
    Rule.prototype.toggleEdit = function () {
        this.editing = !this.editing;

        this.render();
    }
    Rule.prototype.toggle = function() {
        this.$element.toggle();
    }
    Rule.prototype.setModel = function (model) {
        this.options.model = model;
        this.render();
    }
    Rule.prototype.getModel = function () {
        return this.options.model
    }
    Rule.prototype.addKeyword = function (data) {
        if (data.Content.length > 10) {
            $.error('关键字长度最大为10');
            return;
        }
        //if (this.$element.find('.ar_kd_m a').length >= 4) {
        //    //$(this.$element.find('.ar_kd_m input')).hide();
        //}
        if (this.$element.find('.ar_kd_m a').length >= 5) {
            $.error('最多添加5个关键字')
            return;
        }
        var input = this.$element.find('.ar_kd_m input');
        var obj = $('<a><em>' + data.Content + '</em><i>x</i></a>').insertAfter(input);
        obj.find('i').click(function () {
            var keyword = obj.data('rule.keyword');
            if (keyword && keyword.ID) {
                var removedKeyword = input.data('rule.removedKeyword');
                if (!removedKeyword) {
                    input.data('rule.removedKeyword', (removedKeyword = []))
                }
                removedKeyword.push(keyword);
            }
            obj.remove();
            //input.show();
        })
        obj.data('rule.keyword', data);
    }
    Rule.prototype.addResource = function (data) {
        var type = data.type;
        var me = this;
        var target = $('<li></li>').appendTo(this.$element.find('.ar_re_m'));
        target.data('rule.resource', data);
        switch (type + '') {
            //文字 
            case '1':
                target.append('<div class="media_content">' + data.data.Content + '</div>' +
            '<div class="optr"><a href="javascript:"data-action="edit" title="编辑" ><i class="fa fa-pencil" ></i></a>' +
            '<a href="javascript:" title="删除" data-action="delete"><i class="fa fa-trash-o" ></i></a></div>');
                break;
                //图文
            case '2':
                break;
                //语音
            case '3':
                target.append('<div class="media_content"><audio controls="controls"><source src="' + data.data.Path + '" type="audio/mpeg"></audio></div>' +
            '<div class="optr">' +
            '<a href="javascript:" title="删除" data-action="delete"><i class="fa fa-trash-o" ></i></a></div>');
                break;
                //视频
            case '4':
                target.append('<div class="media_content"><video width="320" height="240" controls="controls"><source src="' + data.data.Path + '" type="video/mp4"></video></div>' +
            '<div class="optr">' +
            '<a href="javascript:" title="删除" data-action="delete"><i class="fa fa-trash-o" ></i></a></div>');
                break;
                //图片
            case '5':
                target.append('<div class="media_content"><img style="max-width:100px;max-height:70px;"src="' + data.data.Path + '"/></div>' +
            '<div class="optr">' +
            '<a href="javascript:" title="删除" data-action="delete"><i class="fa fa-trash-o" ></i></a></div>');
                break;
        }
        target.find('a').click(function() {
            var a = $(this);
            if (a.attr('data-action') == 'delete') {
                if (data.data.ID) {
                    //记录下被删除的项
                    var deletedResources = me.$element.data('rule.deletedResources');
                    if (!deletedResources) {
                        me.$element.data('rule.deletedResources', (deletedResources = []))
                    }
                    deletedResources.push(data);
                }
                target.remove();
                //删除
            }else if (a.attr('data-action') == 'edit') {
                modal({
                    title: '修改回复文字',
                    content: '<textarea style="width:100%;height:100px;resize:none;" placeholder="请添加回复文字，最多250个字符">' + target.find('.media_content').html() + '</textarea>',
                    save: function (t) {
                        var text = t.find('textarea').val();
                        if (!text) {
                            $.error('回复不能为空');
                            return;
                        }
                        if (text.length > 250) {
                            $.error('最多回复250个字符');
                            return;
                        }
                        target.find('.media_content').html(text);
                        t.modal('hide');
                    }
                })
            }
        })
    }
    Rule.prototype.getDeletedResources = function() {
        return this.$element.data('rule.deletedResources');
    }
    Rule.prototype.getDeletedKeywords = function() {
        return this.$element.find('.ar_kd_m input').data('rule.removedKeyword');
    }
    Rule.prototype.getResource = function() {
        var resources = [];
        this.$element.find('.ar_re_m li').each(function(i, t) {
            resources.push($(t).data('rule.resource'));
        })
        return resources;
    }
    Rule.prototype.updateModel = function() {
        var model = this.getModel() || {};
        model.Name = this.$element.find('.ar_re_t input').val();
        model.Keywords = this.getKeywords();
        model.ReplyAll = false;
        model.TextReply = [];
        model.ResourceItems = [];
        model.DeletedKeywords = [];
        model.DeletedTextReplies = [];
        model.DeletedResourceItems = [];
        var resources = this.getResource();
        if (resources) {
            $.each(resources, function(i, r) {
                if (r.type == '1') {
                    model.TextReply.push(r.data);
                } else {
                    model.ResourceItems.push(r.data);
                }
            })
        }
        var deletedKeywords = this.getDeletedKeywords();
        if (deletedKeywords) {
            $.each(deletedKeywords, function(i, d) {
                model.DeletedKeywords.push(d.ID);
            })
        }
        var deltedRes = this.getDeletedResources();
        if (deltedRes) {
            $.each(deltedRes, function (i, d) {
                if (d.type == '1') {
                    model.DeletedTextReplies.push(d.data.ID);
                } else {
                    model.DeletedResourceItems.push(d.data.ID);
                }
            })
        }
        return model
    }
    Rule.prototype.getKeywords = function() {
        var keywords = [];
        this.$element.find('.ar_kd_m a').each(function(idx, t) {
            keywords.push($(t).data('rule.keyword'))
        })
        return keywords;
    }
    var old = $.fn.rule

    $.fn.addRule = function (option) {
        var $this = $(this)
        var options = typeof option == 'object' && option
        return new Rule(this, options);
    }

    $.fn.addRule.Constructor = Rule


    // TOOLTIP NO CONFLICT
    // ===================

    $.fn.addRule.noConflict = function () {
        $.fn.addRule = old
        return this
    }
    $.format = function () {
        if (arguments.length == 0)
            return null;

        var str = arguments[0];
        for (var i = 1; i < arguments.length; i++) {
            var re = new RegExp('\\{' + (i - 1) + '\\}', 'gm');
            str = str.replace(re, arguments[i]);
        }
        return str;
    }
})(window.jQuery);