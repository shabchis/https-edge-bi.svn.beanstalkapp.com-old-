/**
 * name:dataMerge
 * description:
 * author:fanxiong<qian.xiong2005@gmail.com>
 * version:1.0
 */
jQuery.fn.extend({
	datamerge:function(){
		function setConditions(){
			;
		}
		//key:数据中的condition; length:jQuery匹配到的元素数量;
		var i=0, length=this.length, arg=arguments[0], key, keylen=(typeof arg.conditions=="object")?arg.conditions.length:0,
		rlen=(typeof arg.result=="object")?arg.result.length:0, mergeValue=(typeof arg.mergeValue=="boolean")?arg.mergeValue:true,
		delArr=[], calcount=0;
		iLoop:for(;i<length-1;i++){
			//如果以前已经比较过了，这次就跳过
			for(var t=0;t<delArr.length;t++){
				if(delArr[t] == i)
					continue iLoop;
			}
			var j=length-1;
			jLoop:for(;j>i;j--){
				//如果以前已经比较过了，这次就跳过
				for(var t=0;t<delArr.length;t++){
					if(delArr[t] == j)
						continue jLoop;
				}
				var isThisEqual=true;
				for(var o=0; o<keylen; o++){
					key=arg.conditions[o];
					var fromlen = ((typeof key.from=="string")?1:(typeof key.from=="object")?key.from.length:0);
					for(var p=0; p<fromlen; p++){
						var subkey=(key.from[p]==undefined) ? key.from : key.from[p];
						var vb = jQuery(this[j]).find("["+key.by+"="+subkey+"]").val();
						var va = jQuery(this[i]).find("["+key.by+"="+subkey+"]").val();
						if(va != vb)
							isThisEqual=false;
						calcount++;
					}
				}
				//如果有相同的，则根据"mergeValue"属性设置决定是否合并值
				if(mergeValue && isThisEqual){
					for(var o=0; o<rlen; o++){
						key=arg.result[o];
						var tolen = ((typeof key.to=="string")?1:(typeof key.to=="object")?key.to.length:0);
						for(var p=0; p<tolen; p++){
							var subkey=key.to[p], vMerged;
							var va = jQuery.trim(jQuery(this[i]).find("["+key.by+"="+subkey+"]").val());
							var vb = jQuery.trim(jQuery(this[j]).find("["+key.by+"="+subkey+"]").val());
							//TODO 根据实际情况决定合并后的值类型,而不只是数字类型
							key.type = (typeof key.type=="string")?key.type:"string";
							switch (key.type.toLowerCase()){
								case "number": case "float": case "int": case "integer":
									va=(va==""|| parseFloat(va)!=va)?0:va;
									vb=(vb==""|| parseFloat(vb)!=vb)?0:vb;
								case "number": case "float":
									vMerged=parseFloat(va)+parseFloat(vb);
									break;
								case "int": case "integer":
									vMerged=parseInt(va)+parseInt(vb);
									break;
								default: case "string":
									vMerged=va+vb;
									break;
							}
							jQuery(this[i]).find("["+key.by+"="+subkey+"]").val(vMerged);
							//删除后面的元素(这里的删除并不能影响jQuery对象内容，虽然页面元素已被删除)
							//jQuery(this[j]).remove();
							//length--;
							delArr.push(j);
						}
					}
				}
				//TODO 如果不需要合并值，那么以什么位置的数据为依据
			}
		}
		//将刚才记录的索引从页面删除
		while(delArr.length){
			jQuery(this[delArr[delArr.length-1]]).remove();
			delArr.pop();
		}
		jQuery("#calcount").html("循环次数:"+calcount);
	},
});
