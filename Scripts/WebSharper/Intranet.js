(function()
{
 var Global=this,Runtime=this.IntelliFactory.Runtime,Concurrency,Remoting,AjaxRemotingProvider,window,Forms,Pervasives,Form,Validation,List,Bootstrap,Controls,Simple,UI,Next,AttrProxy,Doc,AttrModule,Var,Submitter1,View,T,View1,Result1,Doc1,Seq,View2,Attr,ErrorMessage,Form1,Fresh,Result,Utils,Dependent1,Dependent,Ref,Var1,Many,Fresh1,Submitter,Many1,ItemOperations,Collections,ResizeArray,ResizeArrayProxy,Array,CollectionWithDefault,Collection,Operators,Unchecked,Dictionary,RegExp;
 Runtime.Define(Global,{
  Intranet:{
   Client:{
    AnonymousUser:function()
    {
     var _arg00_,_arg00_1,_arg10_,_arg10_1,_arg10_2,x,renderFunction;
     _arg00_=function(userpass)
     {
      var arg00;
      arg00=Concurrency.Delay(function()
      {
       return Concurrency.Bind(AjaxRemotingProvider.Async("Intranet:1",[userpass]),function()
       {
        return Concurrency.Return(window.location.reload());
       });
      });
      return Concurrency.Start(arg00,{
       $:0
      });
     };
     _arg10_=Form.Yield("");
     _arg10_1=Form.Yield("");
     _arg00_1=Pervasives.op_LessMultiplyGreater(Pervasives.op_LessMultiplyGreater(Form.Return(function(user)
     {
      return function(pass)
      {
       return{
        User:user,
        Password:pass
       };
      };
     }),Validation.IsNotEmpty("Must enter a username",_arg10_)),Validation.IsNotEmpty("Must enter a password",_arg10_1));
     _arg10_2=Form.WithSubmit(_arg00_1);
     x=Form.Run(_arg00_,_arg10_2);
     renderFunction=function(user)
     {
      return function(pass)
      {
       return function(submit)
       {
        var arg20;
        arg20=List.ofArray([Simple.InputWithError("Username",user,submit.get_View()),Simple.InputPasswordWithError("Password",pass,submit.get_View()),(((Controls.Button())("Log in"))(List.ofArray([AttrProxy.Create("class","btn btn-primary")])))(function()
        {
         return submit.Trigger();
        }),Controls.ShowErrors(List.ofArray([AttrProxy.Create("style","margin-top:1em")]),submit.get_View())]);
        return Doc.Element("form",[],arg20);
       };
      };
     };
     return Form.Render(renderFunction,x);
    },
    LoggedInUser:function(username)
    {
     var arg20,arg201;
     arg201=List.ofArray([Doc.TextNode("Click here to log out:")]);
     arg20=List.ofArray([Doc.Element("p",[],arg201),Doc.Element("button",List.ofArray([AttrModule.Handler("click",function()
     {
      return function()
      {
       var arg00;
       arg00=Concurrency.Delay(function()
       {
        return Concurrency.Bind(AjaxRemotingProvider.Async("Intranet:2",[username]),function()
        {
         return Concurrency.Return(window.location.reload());
        });
       });
       return Concurrency.Start(arg00,{
        $:0
       });
      };
     })]),List.ofArray([Doc.TextNode("Logout")]))]);
     return Doc.Element("div",[],arg20);
    },
    Main:function()
    {
     var rvInput,submit,arg00,arg10,vReversed,arg20,arg201,arg202,arg203;
     rvInput=Var.Create("");
     submit=Submitter1.CreateOption(rvInput.get_View());
     arg00=function(_arg1)
     {
      return _arg1.$==1?AjaxRemotingProvider.Async("Intranet:0",[_arg1.$0]):Concurrency.Delay(function()
      {
       return Concurrency.Return("");
      });
     };
     arg10=submit.get_View();
     vReversed=View.MapAsync(arg00,arg10);
     arg201=function()
     {
      return submit.Trigger();
     };
     arg202=Runtime.New(T,{
      $:0
     });
     arg203=List.ofArray([Doc.TextView(vReversed)]);
     arg20=List.ofArray([Doc.Input(Runtime.New(T,{
      $:0
     }),rvInput),Doc.Button("Send",Runtime.New(T,{
      $:0
     }),arg201),Doc.Element("hr",[],arg202),Doc.Element("h4",List.ofArray([AttrProxy.Create("class","text-muted")]),List.ofArray([Doc.TextNode("The server responded:")])),Doc.Element("div",List.ofArray([AttrProxy.Create("class","jumbotron")]),List.ofArray([Doc.Element("h1",[],arg203)]))]);
     return Doc.Element("div",[],arg20);
    }
   }
  },
  WebSharper:{
   Forms:{
    Attr:{
     SubmitterValidate:function(submitter)
     {
      var arg00,arg001,arg002,arg10,arg101,arg102;
      arg00=AttrModule.Handler("click",function()
      {
       return function()
       {
        return submitter.Trigger();
       };
      });
      arg001=View1.Const("disabled");
      arg002=function(arg003)
      {
       return Result1.IsFailure(arg003);
      };
      arg10=submitter.get_Input();
      arg101=View.Map(arg002,arg10);
      arg102=AttrModule.DynamicPred("disabled",arg101,arg001);
      return AttrProxy.Append(arg00,arg102);
     }
    },
    Bootstrap:{
     Controls:{
      Button:Runtime.Field(function()
      {
       return function(caption)
       {
        return function(_arg10_)
        {
         return function(_arg20_)
         {
          return Doc.Button(caption,_arg10_,_arg20_);
         };
        };
       };
      }),
      Checkbox:function(lbl,extras,target,labelExtras,targetExtras)
      {
       return Doc.Element("div",Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("checkbox"),
        $1:extras
       }),List.ofArray([Doc.Element("label",labelExtras,List.ofArray([Doc.CheckBox(targetExtras,target),Doc.TextNode(lbl)]))]));
      },
      Class:Runtime.Field(function()
      {
       return function(name)
       {
        return AttrModule.Class(name);
       };
      }),
      Input:function(lbl,extras,target,labelExtras,targetExtras)
      {
       return Doc.Element("div",Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("form-group"),
        $1:extras
       }),List.ofArray([Doc.Element("label",labelExtras,List.ofArray([Doc.TextNode(lbl)])),Doc.Input(Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("form-control"),
        $1:targetExtras
       }),target)]));
      },
      InputPassword:function(lbl,extras,target,labelExtras,targetExtras)
      {
       return Doc.Element("div",Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("form-group"),
        $1:extras
       }),List.ofArray([Doc.Element("label",labelExtras,List.ofArray([Doc.TextNode(lbl)])),Doc.PasswordBox(Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("form-control"),
        $1:targetExtras
       }),target)]));
      },
      InputPasswordWithError:function(lbl)
      {
       var inputFun;
       inputFun=function(_arg00_)
       {
        return function(_arg10_)
        {
         return Doc.PasswordBox(_arg00_,_arg10_);
        };
       };
       return function(extras)
       {
        return function(tupledArg)
        {
         var target,labelExtras,targetExtras;
         target=tupledArg[0];
         labelExtras=tupledArg[1];
         targetExtras=tupledArg[2];
         return function(submitView)
         {
          return Controls.__InputWithError(inputFun,lbl,extras,target,labelExtras,targetExtras,submitView);
         };
        };
       };
      },
      InputWithError:function(lbl)
      {
       var inputFun;
       inputFun=function(_arg00_)
       {
        return function(_arg10_)
        {
         return Doc.Input(_arg00_,_arg10_);
        };
       };
       return function(extras)
       {
        return function(tupledArg)
        {
         var target,labelExtras,targetExtras;
         target=tupledArg[0];
         labelExtras=tupledArg[1];
         targetExtras=tupledArg[2];
         return function(submitView)
         {
          return Controls.__InputWithError(inputFun,lbl,extras,target,labelExtras,targetExtras,submitView);
         };
        };
       };
      },
      Radio:function(lbl,extras,target,labelExtras,targetExtras)
      {
       return Doc.Element("div",Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("radio"),
        $1:extras
       }),List.ofArray([Doc.Element("label",labelExtras,List.ofArray([Doc.Radio(targetExtras,true,target),Doc.TextNode(lbl)]))]));
      },
      ShowErrors:function(extras,submit)
      {
       return Doc1.ShowErrors(submit,function(_arg1)
       {
        var _,mapping,source,ats;
        if(_arg1.$==0)
         {
          _=Doc.get_Empty();
         }
        else
         {
          mapping=function(m)
          {
           var arg20;
           arg20=List.ofArray([Doc.TextNode(m.get_Text())]);
           return Doc.Element("p",[],arg20);
          };
          source=Seq.map(mapping,_arg1);
          ats=List.ofArray([(Controls.cls())("alert alert-danger")]);
          _=Doc.Element("div",extras,List.ofArray([Doc.Element("div",ats,source)]));
         }
        return _;
       });
      },
      Simple:{
       Button:function(label,f)
       {
        return(((Controls.Button())(label))(List.ofArray([(Controls.cls())("button btn-primary")])))(f);
       },
       Checkbox:function(lbl,target)
       {
        return Controls.Checkbox(lbl,Runtime.New(T,{
         $:0
        }),target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        }));
       },
       Input:function(lbl,target)
       {
        return Controls.Input(lbl,Runtime.New(T,{
         $:0
        }),target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        }));
       },
       InputPassword:function(lbl,target)
       {
        return Controls.InputPassword(lbl,Runtime.New(T,{
         $:0
        }),target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        }));
       },
       InputPasswordWithError:function(lbl,target,submit)
       {
        var clo1,arg10,clo2,tupledArg,arg20,arg21,arg22;
        clo1=Controls.InputPasswordWithError(lbl);
        arg10=Runtime.New(T,{
         $:0
        });
        clo2=clo1(arg10);
        tupledArg=[target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        })];
        arg20=tupledArg[0];
        arg21=tupledArg[1];
        arg22=tupledArg[2];
        return(clo2([arg20,arg21,arg22]))(submit);
       },
       InputWithError:function(lbl,target,submit)
       {
        var clo1,arg10,clo2,tupledArg,arg20,arg21,arg22;
        clo1=Controls.InputWithError(lbl);
        arg10=Runtime.New(T,{
         $:0
        });
        clo2=clo1(arg10);
        tupledArg=[target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        })];
        arg20=tupledArg[0];
        arg21=tupledArg[1];
        arg22=tupledArg[2];
        return(clo2([arg20,arg21,arg22]))(submit);
       },
       ShowErrors:function(submit)
       {
        return Controls.ShowErrors(Runtime.New(T,{
         $:0
        }),submit);
       },
       TextArea:function(lbl,target)
       {
        return Controls.TextArea(lbl,Runtime.New(T,{
         $:0
        }),target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        }));
       },
       TextAreaWithError:function(lbl,target,submit)
       {
        var clo1,arg10,clo2,tupledArg,arg20,arg21,arg22;
        clo1=Controls.TextAreaWithError(lbl);
        arg10=Runtime.New(T,{
         $:0
        });
        clo2=clo1(arg10);
        tupledArg=[target,Runtime.New(T,{
         $:0
        }),Runtime.New(T,{
         $:0
        })];
        arg20=tupledArg[0];
        arg21=tupledArg[1];
        arg22=tupledArg[2];
        return(clo2([arg20,arg21,arg22]))(submit);
       }
      },
      TextArea:function(lbl,extras,target,labelExtras,targetExtras)
      {
       return Doc.Element("div",Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("form-group"),
        $1:extras
       }),List.ofArray([Doc.Element("label",labelExtras,List.ofArray([Doc.TextNode(lbl)])),Doc.InputArea(Runtime.New(T,{
        $:1,
        $0:(Controls.cls())("form-control"),
        $1:targetExtras
       }),target)]));
      },
      TextAreaWithError:function(lbl)
      {
       var inputFun;
       inputFun=function(_arg00_)
       {
        return function(_arg10_)
        {
         return Doc.InputArea(_arg00_,_arg10_);
        };
       };
       return function(extras)
       {
        return function(tupledArg)
        {
         var target,labelExtras,targetExtras;
         target=tupledArg[0];
         labelExtras=tupledArg[1];
         targetExtras=tupledArg[2];
         return function(submitView)
         {
          return Controls.__InputWithError(inputFun,lbl,extras,target,labelExtras,targetExtras,submitView);
         };
        };
       };
      },
      __InputWithError:function(inputFun,lbl,extras,target,labelExtras,targetExtras,submitView)
      {
       var tv,view,patternInput,errorOpt,errorClassOpt,ats;
       tv=View2.Through1(submitView,target);
       view=View.Map(function(res)
       {
        var _,_1,errs,mapping,reduction,list,errors;
        if(res.$==1)
         {
          if(res.$0.$==0)
           {
            _1=[{
             $:0
            },{
             $:0
            }];
           }
          else
           {
            errs=res.$0;
            mapping=function(e)
            {
             return e.get_Text();
            };
            reduction=function(a)
            {
             return function(b)
             {
              return a+"; "+b;
             };
            };
            list=List.map(mapping,errs);
            errors=Seq.reduce(reduction,list);
            _1=[{
             $:1,
             $0:errors
            },{
             $:1,
             $0:"has-error"
            }];
           }
          _=_1;
         }
        else
         {
          _=[{
           $:0
          },{
           $:0
          }];
         }
        return _;
       },tv);
       patternInput=[View.Map(function(tuple)
       {
        return tuple[0];
       },view),View.Map(function(tuple)
       {
        return tuple[1];
       },view)];
       errorOpt=patternInput[0];
       errorClassOpt=patternInput[1];
       ats=Seq.toList(Seq.delay(function()
       {
        return Seq.append([(Controls.cls())("form-group")],Seq.delay(function()
        {
         return Seq.append([AttrModule.DynamicClass("has-error",errorClassOpt,function(opt)
         {
          return opt.$==1;
         })],Seq.delay(function()
         {
          return extras;
         }));
        }));
       }));
       return Doc.Element("div",ats,Seq.toList(Seq.delay(function()
       {
        return Seq.append([Doc.Element("label",labelExtras,List.ofArray([Doc.TextNode(lbl)]))],Seq.delay(function()
        {
         return Seq.append([(inputFun(Runtime.New(T,{
          $:1,
          $0:(Controls.cls())("form-control"),
          $1:targetExtras
         })))(target)],Seq.delay(function()
         {
          return[Doc.BindView(function(_arg1)
          {
           var _,error;
           if(_arg1.$==1)
            {
             error=_arg1.$0;
             _=Doc.Element("span",List.ofArray([(Controls.cls())("help-block")]),List.ofArray([Doc.TextNode(error)]));
            }
           else
            {
             _=Doc.get_Empty();
            }
           return _;
          },errorOpt)];
         }));
        }));
       })));
      },
      cls:Runtime.Field(function()
      {
       return function(name)
       {
        return AttrModule.Class(name);
       };
      })
     }
    },
    Doc:{
     ButtonValidate:function(caption,attrs,submitter)
     {
      return Doc.Element("button",Seq.append([Attr.SubmitterValidate(submitter)],attrs),List.ofArray([Doc.TextNode(caption)]));
     },
     ShowErrors:function(v,f)
     {
      return Doc.BindView(function(_arg1)
      {
       var _,msgs;
       if(_arg1.$==1)
        {
         msgs=_arg1.$0;
         _=f(msgs);
        }
       else
        {
         _=Doc.get_Empty();
        }
       return _;
      },v);
     },
     ShowSuccess:function(v,f)
     {
      return Doc.BindView(function(_arg1)
      {
       var _,x;
       if(_arg1.$==1)
        {
         _arg1.$0;
         _=Doc.get_Empty();
        }
       else
        {
         x=_arg1.$0;
         _=f(x);
        }
       return _;
      },v);
     }
    },
    ErrorMessage:Runtime.Class({
     get_Id:function()
     {
      return this.id;
     },
     get_Text:function()
     {
      return this.message;
     }
    },{
     Create:function(p,text)
     {
      return ErrorMessage.New(p.id,text);
     },
     Create1:function(id,text)
     {
      return ErrorMessage.New(id,text);
     },
     New:function(id,message)
     {
      var r;
      r=Runtime.New(this,{});
      r.id=id;
      r.message=message;
      return r;
     }
    }),
    Form:{
     ApJoin:function(pf,px)
     {
      var arg00,arg101,arg20,f,g;
      arg00=function(arg001)
      {
       return function(arg10)
       {
        return Result.ApJoin(arg001,arg10);
       };
      };
      arg101=pf.view;
      arg20=px.view;
      f=pf.render;
      g=px.render;
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:View.Map2(arg00,arg101,arg20),
       render:function(x)
       {
        return g(f(x));
       }
      });
     },
     Apply:function(pf,px)
     {
      var arg00,arg101,arg20,f,g;
      arg00=function(arg001)
      {
       return function(arg10)
       {
        return Result.Apply(arg001,arg10);
       };
      };
      arg101=pf.view;
      arg20=px.view;
      f=pf.render;
      g=px.render;
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:View.Map2(arg00,arg101,arg20),
       render:function(x)
       {
        return g(f(x));
       }
      });
     },
     Builder:Runtime.Class({
      Bind:function(p,f)
      {
       return Form.Dependent2(p,f);
      },
      Return:function(x)
      {
       return Form.Return(x);
      },
      ReturnFrom:function(p)
      {
       return p;
      },
      Yield:function(x)
      {
       return Form.Yield(x);
      },
      YieldFrom:function(p)
      {
       return p;
      },
      Zero:function()
      {
       return Form.ReturnFailure();
      }
     }),
     Create:function(view,renderBuilder)
     {
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:view,
       render:renderBuilder
      });
     },
     Dependent:{
      Make:function(primary,dependent)
      {
       var dependent1,pOut;
       dependent1=Utils.memoize(function(x)
       {
        var p,view;
        p=dependent(x);
        view=p.view;
        return Runtime.New(Form1,{
         id:p.id,
         view:view,
         render:function(x1)
         {
          return p.render.call(null,x1);
         }
        });
       });
       pOut=View.Map(function(arg10)
       {
        return Result1.Map(dependent1,arg10);
       },primary.view);
       return Dependent1.New(function(x)
       {
        return primary.render.call(null,x);
       },pOut);
      }
     },
     Dependent1:Runtime.Class({
      RenderDependent:function(f)
      {
       var _arg00_,_arg10_;
       _arg00_=function(_arg2)
       {
        var _,p;
        if(_arg2.$==1)
         {
          _=Doc.get_Empty();
         }
        else
         {
          p=_arg2.$0;
          _=p.render.call(null,f);
         }
        return _;
       };
       _arg10_=this.pOut;
       return Doc.BindView(_arg00_,_arg10_);
      },
      RenderPrimary:function(f)
      {
       return this.renderPrimary.call(null,f);
      },
      get_View:function()
      {
       return this.out;
      }
     },{
      New:function(renderPrimary,pOut)
      {
       var r;
       r=Runtime.New(this,{});
       r.renderPrimary=renderPrimary;
       r.pOut=pOut;
       r.out=View1.Bind(function(_arg1)
       {
        var _,m,p;
        if(_arg1.$==1)
         {
          m=_arg1.$0;
          _=View1.Const({
           $:1,
           $0:m
          });
         }
        else
         {
          p=_arg1.$0;
          _=p.view;
         }
        return _;
       },r.pOut);
       return r;
      }
     }),
     Dependent2:function(input,output)
     {
      var d;
      d=Dependent.Make(input,output);
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:d.get_View(),
       render:function(f)
       {
        return f(d);
       }
      });
     },
     FlushErrors:function(p)
     {
      return Form.MapResult(function(_arg1)
      {
       return _arg1.$==1?{
        $:1,
        $0:Runtime.New(T,{
         $:0
        })
       }:_arg1;
      },p);
     },
     GetView:function(p)
     {
      return p.view;
     },
     Many:{
      Fresh:{
       Int:Runtime.Field(function()
       {
        var x;
        x=[0];
        return function()
        {
         Ref.incr(x);
         return x[0];
        };
       })
      }
     },
     Many1:{
      Collection:Runtime.Class({
       Add:function(x)
       {
        return this.add(x);
       },
       Render:function(f)
       {
        var x,_arg00_,_arg10_;
        x=this.changesView;
        _arg00_=function(tupledArg)
        {
         var ident;
         tupledArg[0];
         tupledArg[1];
         ident=tupledArg[2];
         return ident;
        };
        _arg10_=function(tupledArg)
        {
         var p,ops;
         p=tupledArg[0];
         ops=tupledArg[1];
         tupledArg[2];
         return p.render.call(null,f(ops));
        };
        return Doc.ConvertBy(_arg00_,_arg10_,x);
       },
       RenderAdder:function(f)
       {
        var x,arg00,_this=this,arg10,_arg00_,arg001;
        x=this.adder.render.call(null,f);
        arg00=function(x1)
        {
         return _this.adderView(x1);
        };
        arg10=_this.adder.view;
        _arg00_=View.Map(arg00,arg10);
        arg001=Doc.EmbedView(_arg00_);
        return Doc.Append(arg001,x);
       },
       add:function(x)
       {
        var arg00,arg10;
        this.arr.Add(this.mk(x));
        arg00=this["var"];
        arg10=function(x1)
        {
         return x1;
        };
        return Var1.Update(arg00,arg10);
       },
       adderView:function(x)
       {
        var _,x1;
        if(x.$==0)
         {
          x1=x.$0;
          _=this.add(x1);
         }
        else
         {
          _=null;
         }
        return Doc.get_Empty();
       },
       get_View:function()
       {
        return this.out;
       },
       mk:function(x)
       {
        var ident,predicate,getThisIndexIn,arg10,vIndex,_delete,_this=this,arg001,inp,arg102,sMoveUp,x2,arg002,vMoveUp,arg003,inp1,arg103,sMoveDown,x3,arg004,vMoveDown,p,arg005,arg104,arg006,arg20,v,p1;
        ident=(Fresh1.Int())(null);
        predicate=function(tupledArg)
        {
         var j;
         tupledArg[0];
         tupledArg[1];
         j=tupledArg[2];
         return ident===j;
        };
        getThisIndexIn=function(source)
        {
         return Seq.findIndex(predicate,source);
        };
        arg10=this["var"].get_View();
        vIndex=View.Map(getThisIndexIn,arg10);
        _delete=function()
        {
         var k,arg00,arg101;
         k=getThisIndexIn(_this.arr);
         _this.arr.RemoveAt(k);
         arg00=_this["var"];
         arg101=function(x1)
         {
          return x1;
         };
         return Var1.Update(arg00,arg101);
        };
        arg001=function(i)
        {
         return i===0?{
          $:1,
          $0:Runtime.New(T,{
           $:0
          })
         }:{
          $:0,
          $0:true
         };
        };
        inp=View.Map(arg001,vIndex);
        arg102=_this.arr.get_Count()===0?{
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        }:{
         $:0,
         $0:false
        };
        sMoveUp=Submitter.Create(inp,arg102);
        x2=sMoveUp.get_View();
        arg002=function(_arg1)
        {
         var _,_1,i,arg00,arg101;
         if(_arg1.$==0)
          {
           if(_arg1.$0)
            {
             i=getThisIndexIn(_this.arr);
             Many1["List`1.Swap"](_this.arr,i,i-1);
             arg00=_this["var"];
             arg101=function(x1)
             {
              return x1;
             };
             _1=Var1.Update(arg00,arg101);
            }
           else
            {
             _1=null;
            }
           _=_1;
          }
         else
          {
           _=null;
          }
         return _;
        };
        vMoveUp=View.Map(arg002,x2);
        arg003=function(i)
        {
         return i===_this.arr.get_Count()-1?{
          $:1,
          $0:Runtime.New(T,{
           $:0
          })
         }:{
          $:0,
          $0:true
         };
        };
        inp1=View.Map(arg003,vIndex);
        arg103={
         $:1,
         $0:Runtime.New(T,{
          $:0
         })
        };
        sMoveDown=Submitter.Create(inp1,arg103);
        x3=sMoveDown.get_View();
        arg004=function(_arg2)
        {
         var _,_1,i,arg00,arg101;
         if(_arg2.$==0)
          {
           if(_arg2.$0)
            {
             i=getThisIndexIn(_this.arr);
             Many1["List`1.Swap"](_this.arr,i,i+1);
             arg00=_this["var"];
             arg101=function(x1)
             {
              return x1;
             };
             _1=Var1.Update(arg00,arg101);
            }
           else
            {
             _1=null;
            }
           _=_1;
          }
         else
          {
           _=null;
          }
         return _;
        };
        vMoveDown=View.Map(arg004,x3);
        p=_this.p.call(null,x);
        arg005=function(x1)
        {
         return function()
         {
          return x1;
         };
        };
        arg104=p.view;
        arg006=function()
        {
         return function()
         {
          return null;
         };
        };
        arg20=View.Map2(arg006,vMoveUp,vMoveDown);
        v=View.Map2(arg005,arg104,arg20);
        p1=Runtime.New(Form1,{
         id:p.id,
         view:v,
         render:p.render
        });
        return[p1,ItemOperations.New(_delete,sMoveUp,sMoveDown),ident];
       }
      },{
       New:function(p,inits,adder)
       {
        var r,objectArg,x1,arg001,x2,arg003;
        r=Runtime.New(this,{});
        r.p=p;
        r.adder=adder;
        r.arr=ResizeArrayProxy.New2();
        r["var"]=Var.Create(r.arr);
        objectArg=r.arr;
        Seq.iter(function(x)
        {
         var arg00;
         arg00=r.mk(x);
         return objectArg.Add(arg00);
        },inits);
        x1=r["var"].get_View();
        arg001=function(arr)
        {
         var x,f,z,arg002,re;
         x=arr.ToArray();
         f=function(_arg4)
         {
          var p1,arg00,arg10;
          p1=_arg4[0];
          arg00=function()
          {
           return[_arg4];
          };
          arg10=p1.view;
          return View.Map(arg00,arg10);
         };
         z=View1.Const(Seq.empty());
         arg002=function(source1)
         {
          return function(source2)
          {
           return Seq.append(source1,source2);
          };
         };
         re=function(arg10)
         {
          return function(arg20)
          {
           return View.Map2(arg002,arg10,arg20);
          };
         };
         return Array.MapReduce(f,z,re,x);
        };
        r.changesView=View1.Bind(arg001,x1);
        x2=r["var"].get_View();
        arg003=function(s)
        {
         var x,f,z,arg004,arg005,re;
         x=s.ToArray();
         f=function(tupledArg)
         {
          var p1,arg00,arg002,arg101;
          p1=tupledArg[0];
          tupledArg[1];
          tupledArg[2];
          arg00=function(value)
          {
           return[value];
          };
          arg002=function(arg10)
          {
           return Result1.Map(arg00,arg10);
          };
          arg101=p1.view;
          return View.Map(arg002,arg101);
         };
         z=View1.Const({
          $:0,
          $0:Seq.empty()
         });
         arg004=function(source1)
         {
          return function(source2)
          {
           return Seq.append(source1,source2);
          };
         };
         arg005=function(arg10)
         {
          return function(arg20)
          {
           return Result.Append(arg004,arg10,arg20);
          };
         };
         re=function(arg10)
         {
          return function(arg20)
          {
           return View.Map2(arg005,arg10,arg20);
          };
         };
         return Array.MapReduce(f,z,re,x);
        };
        r.out=View1.Bind(arg003,x2);
        return r;
       }
      }),
      CollectionWithDefault:Runtime.Class({
       AddOne:function()
       {
        return this.Add(this["default"]);
       }
      },{
       New:function(p,inits,pInit,_default)
       {
        var r;
        r=Runtime.New(this,Collection.New(p,inits,pInit));
        r["default"]=_default;
        return r;
       }
      }),
      ItemOperations:Runtime.Class({
       Delete:function()
       {
        return this["delete"].call(null,null);
       },
       get_MoveDown:function()
       {
        return this.moveDown;
       },
       get_MoveUp:function()
       {
        return this.moveUp;
       }
      },{
       New:function(_delete,moveUp,moveDown)
       {
        var r;
        r=Runtime.New(this,{});
        r["delete"]=_delete;
        r.moveUp=moveUp;
        r.moveDown=moveDown;
        return r;
       }
      }),
      "List`1.Swap":function(_this,i,j)
      {
       var tmp;
       tmp=_this.get_Item(i);
       _this.set_Item(i,_this.get_Item(j));
       return _this.set_Item(j,tmp);
      }
     },
     Many2:function(inits,init,p)
     {
      var pInit,m;
      pInit=p(init);
      m=CollectionWithDefault.New(p,inits,pInit,init);
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:m.get_View(),
       render:function(f)
       {
        return f(m);
       }
      });
     },
     ManyForm:function(inits,create,p)
     {
      var m;
      m=Collection.New(p,inits,create);
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:m.get_View(),
       render:function(f)
       {
        return f(m);
       }
      });
     },
     Map:function(f,p)
     {
      return Form.MapResult(function(arg10)
      {
       return Result1.Map(f,arg10);
      },p);
     },
     MapAsync:function(f,p)
     {
      var f1;
      f1=function(x)
      {
       var _,m,x1;
       if(x.$==1)
        {
         m=x.$0;
         _=Concurrency.Delay(function()
         {
          return Concurrency.Return({
           $:1,
           $0:m
          });
         });
        }
       else
        {
         x1=x.$0;
         _=Concurrency.Delay(function()
         {
          return Concurrency.Bind(f(x1),function(_arg1)
          {
           return Concurrency.Return({
            $:0,
            $0:_arg1
           });
          });
         });
        }
       return _;
      };
      return Form.MapAsyncResult(f1,p);
     },
     MapAsyncResult:function(f,p)
     {
      var arg10;
      arg10=p.view;
      return Runtime.New(Form1,{
       id:p.id,
       view:View.MapAsync(f,arg10),
       render:p.render
      });
     },
     MapRenderArgs:function(f,p)
     {
      return Runtime.New(Form1,{
       id:p.id,
       view:p.view,
       render:function(g)
       {
        return g(p.render.call(null,f));
       }
      });
     },
     MapResult:function(f,p)
     {
      var arg10;
      arg10=p.view;
      return Runtime.New(Form1,{
       id:p.id,
       view:View.Map(f,arg10),
       render:p.render
      });
     },
     MapToAsyncResult:function(f,p)
     {
      var f1;
      f1=function(x)
      {
       var _,m,x1;
       if(x.$==1)
        {
         m=x.$0;
         _=Concurrency.Delay(function()
         {
          return Concurrency.Return({
           $:1,
           $0:m
          });
         });
        }
       else
        {
         x1=x.$0;
         _=Concurrency.Delay(function()
         {
          return f(x1);
         });
        }
       return _;
      };
      return Form.MapAsyncResult(f1,p);
     },
     MapToResult:function(f,p)
     {
      return Form.MapResult(function(arg10)
      {
       return Result.Bind(f,arg10);
      },p);
     },
     Render:function(renderFunction,p)
     {
      var x,arg00,arg10,_arg00_,arg001;
      x=p.render.call(null,renderFunction);
      arg00=function()
      {
       return Doc.get_Empty();
      };
      arg10=p.view;
      _arg00_=View.Map(arg00,arg10);
      arg001=Doc.EmbedView(_arg00_);
      return Doc.Append(arg001,x);
     },
     RenderDependent:function(d,f)
     {
      return d.RenderDependent(f);
     },
     RenderMany:function(c,f)
     {
      return c.Render(f);
     },
     RenderManyAdder:function(c,f)
     {
      return c.RenderAdder(f);
     },
     RenderPrimary:function(d,f)
     {
      return d.RenderPrimary(f);
     },
     Return:function(value)
     {
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:View1.Const({
        $:0,
        $0:value
       }),
       render:function(x)
       {
        return x;
       }
      });
     },
     ReturnFailure:function()
     {
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:View1.Const({
        $:1,
        $0:Runtime.New(T,{
         $:0
        })
       }),
       render:function(x)
       {
        return x;
       }
      });
     },
     Run:function(f,p)
     {
      return Form.Map(function(x)
      {
       f(x);
       return x;
      },p);
     },
     RunResult:function(f,p)
     {
      return Form.MapResult(function(x)
      {
       f(x);
       return x;
      },p);
     },
     TransmitView:function(p)
     {
      return Runtime.New(Form1,{
       id:p.id,
       view:p.view,
       render:function(x)
       {
        return(p.render.call(null,x))(p.view);
       }
      });
     },
     TransmitViewMap:function(f,p)
     {
      return Form.TransmitViewMapResult(function(arg10)
      {
       return Result1.Map(f,arg10);
      },p);
     },
     TransmitViewMapResult:function(f,p)
     {
      return Runtime.New(Form1,{
       id:p.id,
       view:p.view,
       render:function(x)
       {
        var arg10;
        arg10=p.view;
        return(p.render.call(null,x))(View.Map(f,arg10));
       }
      });
     },
     WithSubmit:function(p)
     {
      var arg00,arg10,submitter;
      arg00=p.view;
      arg10={
       $:1,
       $0:Runtime.New(T,{
        $:0
       })
      };
      submitter=Submitter.Create(arg00,arg10);
      return Runtime.New(Form1,{
       id:Fresh.Id(),
       view:submitter.get_View(),
       render:function(r)
       {
        return(p.render.call(null,r))(submitter);
       }
      });
     },
     Yield:function(value)
     {
      return Form.YieldVar(Var.Create(value));
     },
     YieldFailure:function()
     {
      var _var,view,arg00,arg001,arg20;
      _var=Var.Create(undefined);
      view=_var.get_View();
      arg00={
       $:1,
       $0:Runtime.New(T,{
        $:0
       })
      };
      arg001=function(arg0)
      {
       return{
        $:0,
        $0:arg0
       };
      };
      arg20=View.Map(arg001,view);
      return Runtime.New(Form1,{
       id:Var1.GetId(_var),
       view:View.SnapshotOn(arg00,view,arg20),
       render:function(r)
       {
        return r(_var);
       }
      });
     },
     YieldOption:function(init,noneValue)
     {
      var _var,arg00,arg10;
      _var=Var.Create(Operators.DefaultArg(init,noneValue));
      arg00=function(x)
      {
       return{
        $:0,
        $0:Unchecked.Equals(x,noneValue)?{
         $:0
        }:{
         $:1,
         $0:x
        }
       };
      };
      arg10=_var.get_View();
      return Runtime.New(Form1,{
       id:Var1.GetId(_var),
       view:View.Map(arg00,arg10),
       render:function(r)
       {
        return r(_var);
       }
      });
     },
     YieldVar:function(_var)
     {
      var arg00,arg10;
      arg00=function(arg0)
      {
       return{
        $:0,
        $0:arg0
       };
      };
      arg10=_var.get_View();
      return Runtime.New(Form1,{
       id:Var1.GetId(_var),
       view:View.Map(arg00,arg10),
       render:function(r)
       {
        return r(_var);
       }
      });
     }
    },
    Form1:Runtime.Class({
     get_Id:function()
     {
      return this.id;
     },
     get_Render:function()
     {
      return this.render;
     },
     get_View:function()
     {
      return this.view;
     }
    }),
    Fresh:{
     Id:function()
     {
      Ref.decr(Fresh.lastId());
      return(Fresh.lastId())[0];
     },
     lastId:Runtime.Field(function()
     {
      return[-1];
     })
    },
    Pervasives:{
     op_LessMultiplyGreater:function(pf,px)
     {
      return Form.Apply(pf,px);
     },
     op_LessMultiplyQmarkGreater:function(pf,px)
     {
      return Form.ApJoin(pf,px);
     }
    },
    Result:Runtime.Class({},{
     ApJoin:function(rf,rx)
     {
      var _,f,_1,_2,x,mx,mx1,mf,_3,mx2;
      if(rf.$==0)
       {
        f=rf.$0;
        if(rx.$==0)
         {
          if(rx.$0.$==0)
           {
            x=rx.$0.$0;
            _2={
             $:0,
             $0:f(x)
            };
           }
          else
           {
            mx=rx.$0.$0;
            _2={
             $:1,
             $0:mx
            };
           }
          _1=_2;
         }
        else
         {
          mx1=rx.$0;
          _1={
           $:1,
           $0:mx1
          };
         }
        _=_1;
       }
      else
       {
        mf=rf.$0;
        if(rx.$==0)
         {
          _3={
           $:1,
           $0:mf
          };
         }
        else
         {
          mx2=rx.$0;
          _3={
           $:1,
           $0:List.append(mf,mx2)
          };
         }
        _=_3;
       }
      return _;
     },
     Append:function(app,r1,r2)
     {
      var _,x1,_1,x2,m1,_2,m2;
      if(r1.$==0)
       {
        x1=r1.$0;
        if(r2.$==0)
         {
          x2=r2.$0;
          _1={
           $:0,
           $0:(app(x1))(x2)
          };
         }
        else
         {
          _1=r2;
         }
        _=_1;
       }
      else
       {
        m1=r1.$0;
        if(r2.$==0)
         {
          _2=r1;
         }
        else
         {
          m2=r2.$0;
          _2={
           $:1,
           $0:List.append(m1,m2)
          };
         }
        _=_2;
       }
      return _;
     },
     Apply:function(rf,rx)
     {
      var _,f,_1,x,mx,mf,_2,mx1;
      if(rf.$==0)
       {
        f=rf.$0;
        if(rx.$==0)
         {
          x=rx.$0;
          _1={
           $:0,
           $0:f(x)
          };
         }
        else
         {
          mx=rx.$0;
          _1={
           $:1,
           $0:mx
          };
         }
        _=_1;
       }
      else
       {
        mf=rf.$0;
        if(rx.$==0)
         {
          _2={
           $:1,
           $0:mf
          };
         }
        else
         {
          mx1=rx.$0;
          _2={
           $:1,
           $0:List.append(mf,mx1)
          };
         }
        _=_2;
       }
      return _;
     },
     Bind:function(f,rx)
     {
      var _,x,m;
      if(rx.$==0)
       {
        x=rx.$0;
        _=f(x);
       }
      else
       {
        m=rx.$0;
        _={
         $:1,
         $0:m
        };
       }
      return _;
     },
     FailWith:function(errorMessage,id)
     {
      var id1,_,id2;
      if(id.$==0)
       {
        _=Fresh.Id();
       }
      else
       {
        id2=id.$0;
        _=id2;
       }
      id1=_;
      return{
       $:1,
       $0:List.ofArray([ErrorMessage.New(id1,errorMessage)])
      };
     }
    }),
    Result1:Runtime.Class({},{
     IsFailure:function(r)
     {
      return r.$==1?true:false;
     },
     IsSuccess:function(r)
     {
      return r.$==1?false:true;
     },
     Map:function(f,r)
     {
      var _,m,x;
      if(r.$==1)
       {
        m=r.$0;
        _={
         $:1,
         $0:m
        };
       }
      else
       {
        x=r.$0;
        _={
         $:0,
         $0:f(x)
        };
       }
      return _;
     }
    }),
    Utils:{
     memoize:function(f)
     {
      var d;
      d=Dictionary.New12();
      return function(x)
      {
       var _,y;
       if(d.ContainsKey(x))
        {
         _=d.get_Item(x);
        }
       else
        {
         y=f(x);
         d.set_Item(x,y);
         _=y;
        }
       return _;
      };
     }
    },
    Validation:{
     Is:function(pred,msg,p)
     {
      var _arg00_;
      _arg00_=function(res)
      {
       var _,x;
       if(res.$==1)
        {
         _=res;
        }
       else
        {
         x=res.$0;
         _=pred(x)?res:{
          $:1,
          $0:List.ofArray([ErrorMessage.New(p.id,msg)])
         };
        }
       return _;
      };
      return Form.MapResult(_arg00_,p);
     },
     IsMatch:function(re,msg,p)
     {
      var objectArg;
      objectArg=new RegExp(re);
      return Validation.Is(function(arg00)
      {
       return objectArg.test(arg00);
      },msg,p);
     },
     IsNotEmpty:function(msg,p)
     {
      return Validation.Is(function(x)
      {
       return x!=="";
      },msg,p);
     },
     MapValidCheckedInput:function(msg,p)
     {
      var _arg00_;
      _arg00_=function(res)
      {
       var _,msgs,_1,x;
       if(res.$==1)
        {
         msgs=res.$0;
         _={
          $:1,
          $0:msgs
         };
        }
       else
        {
         if(res.$0.$==0)
          {
           x=res.$0.$0;
           _1={
            $:0,
            $0:x
           };
          }
         else
          {
           _1={
            $:1,
            $0:List.ofArray([ErrorMessage.Create(p,msg)])
           };
          }
         _=_1;
        }
       return _;
      };
      return Form.MapResult(_arg00_,p);
     }
    },
    View:Runtime.Class({},{
     Through:function(_this,p)
     {
      var arg00;
      arg00=function(x)
      {
       var _,msgs,predicate;
       if(x.$==1)
        {
         msgs=x.$0;
         predicate=function(m)
         {
          return m.get_Id()===p.id;
         };
         _={
          $:1,
          $0:List.filter(predicate,msgs)
         };
        }
       else
        {
         _=x;
        }
       return _;
      };
      return View.Map(arg00,_this);
     },
     Through1:function(_this,v)
     {
      var arg00;
      arg00=function(x)
      {
       var _,msgs,predicate;
       if(x.$==1)
        {
         msgs=x.$0;
         predicate=function(m)
         {
          return m.get_Id()===Var1.GetId(v);
         };
         _={
          $:1,
          $0:List.filter(predicate,msgs)
         };
        }
       else
        {
         _=x;
        }
       return _;
      };
      return View.Map(arg00,_this);
     }
    })
   }
  }
 });
 Runtime.OnInit(function()
 {
  Concurrency=Runtime.Safe(Global.WebSharper.Concurrency);
  Remoting=Runtime.Safe(Global.WebSharper.Remoting);
  AjaxRemotingProvider=Runtime.Safe(Remoting.AjaxRemotingProvider);
  window=Runtime.Safe(Global.window);
  Forms=Runtime.Safe(Global.WebSharper.Forms);
  Pervasives=Runtime.Safe(Forms.Pervasives);
  Form=Runtime.Safe(Forms.Form);
  Validation=Runtime.Safe(Forms.Validation);
  List=Runtime.Safe(Global.WebSharper.List);
  Bootstrap=Runtime.Safe(Forms.Bootstrap);
  Controls=Runtime.Safe(Bootstrap.Controls);
  Simple=Runtime.Safe(Controls.Simple);
  UI=Runtime.Safe(Global.WebSharper.UI);
  Next=Runtime.Safe(UI.Next);
  AttrProxy=Runtime.Safe(Next.AttrProxy);
  Doc=Runtime.Safe(Next.Doc);
  AttrModule=Runtime.Safe(Next.AttrModule);
  Var=Runtime.Safe(Next.Var);
  Submitter1=Runtime.Safe(Next.Submitter1);
  View=Runtime.Safe(Next.View);
  T=Runtime.Safe(List.T);
  View1=Runtime.Safe(Next.View1);
  Result1=Runtime.Safe(Forms.Result1);
  Doc1=Runtime.Safe(Forms.Doc);
  Seq=Runtime.Safe(Global.WebSharper.Seq);
  View2=Runtime.Safe(Forms.View);
  Attr=Runtime.Safe(Forms.Attr);
  ErrorMessage=Runtime.Safe(Forms.ErrorMessage);
  Form1=Runtime.Safe(Forms.Form1);
  Fresh=Runtime.Safe(Forms.Fresh);
  Result=Runtime.Safe(Forms.Result);
  Utils=Runtime.Safe(Forms.Utils);
  Dependent1=Runtime.Safe(Form.Dependent1);
  Dependent=Runtime.Safe(Form.Dependent);
  Ref=Runtime.Safe(Global.WebSharper.Ref);
  Var1=Runtime.Safe(Next.Var1);
  Many=Runtime.Safe(Form.Many);
  Fresh1=Runtime.Safe(Many.Fresh);
  Submitter=Runtime.Safe(Next.Submitter);
  Many1=Runtime.Safe(Form.Many1);
  ItemOperations=Runtime.Safe(Many1.ItemOperations);
  Collections=Runtime.Safe(Global.WebSharper.Collections);
  ResizeArray=Runtime.Safe(Collections.ResizeArray);
  ResizeArrayProxy=Runtime.Safe(ResizeArray.ResizeArrayProxy);
  Array=Runtime.Safe(Next.Array);
  CollectionWithDefault=Runtime.Safe(Many1.CollectionWithDefault);
  Collection=Runtime.Safe(Many1.Collection);
  Operators=Runtime.Safe(Global.WebSharper.Operators);
  Unchecked=Runtime.Safe(Global.WebSharper.Unchecked);
  Dictionary=Runtime.Safe(Collections.Dictionary);
  return RegExp=Runtime.Safe(Global.RegExp);
 });
 Runtime.OnLoad(function()
 {
  Runtime.Inherit(CollectionWithDefault,Collection);
  Fresh.lastId();
  Fresh1.Int();
  Controls.cls();
  Controls.Class();
  Controls.Button();
  return;
 });
}());
