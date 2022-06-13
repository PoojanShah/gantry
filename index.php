<?
	date_default_timezone_set('America/New_York');
	error_reporting(E_ALL|E_STRICT);
	session_start();
	$CATEGORYFILE="/etc/motions/categories.cfg";
	$UNLOCKFILE="/etc/motions/unlock.cfg";
	$COMMANDFILE="/var/www/run/motions.cmd";
	$CONFIGFILE="/etc/motions/motions.cfg";
	$LOGFILE="/var/log/motions/commands.log";
	#$CATEGORYFILE="/kunden/homepages/2/d88344445/htdocs/motions/categories.cfg";
	#$UNLOCKFILE="/kunden/homepages/2/d88344445/htdocs/motions/unlock.cfg";
	#$COMMANDFILE="/kunden/homepages/2/d88344445/htdocs/motions/cmd";
	#$CONFIGFILE="/kunden/homepages/2/d88344445/htdocs/motions/motions.cfg";
	#$LOGFILE="/kunden/homepages/2/d88344445/htdocs/motions/commands.log";
	#echo "\$_SESSION: ".print_r($_SESSION,true);
	$commands="";
	#foreach($_SESSION as $k=>$v)unset($_SESSION[$k]);
	if(file_exists($CONFIGFILE)){
		if($fh=fopen($CONFIGFILE,"r"))while(($line=fgets($fh))!==FALSE){
			#echo "Processling line: \"$line\"<br>";
			$line=trim($line);
			if(substr($line,0,1)=="#"||$line=="")continue;
			else if(strpos($line,"=")===FALSE)die("Ungueltiges line: \"$line\"\n");
			list($varname,$value)=preg_split("!=!",$line);
			if(empty($value))die("Empty value for variable \"$varname\".");
#			switch($varname){
#				case "commandfile":
#				default:die("Ungueltiges Variable Name: \"$varname\".");
#			}
			$varname=strtoupper($varname);
			$$varname=$value;
			#echo "Set \$$varname to \"$value\".<br>";
		}else die("Error reading existing file \"$CONFIGFILE\".");
	}
	if(!file_exists($CATEGORYFILE))die("Category file not found: \"$CATEGORYFILE\"");
	$catnames=array("boy","girl","man","woman");
	#include_once("srs.php");
	#foreach(array("category","play") as $v)if(isset($_GET[substr($v,0,1)]))$$v=(int)$_GET[substr($v,0,1)];
	if(isset($_GET["back"])){
		unset($_SESSION["category"]);
		unset($_GET["back"]);
	}
	#foreach(array(0,1) as $n)if(isset($_GET["h$n"])||isset($_GET["h-1"]))unset($_SESSION["playing-$n"]);
	foreach(array("category","play","screen","halt") as $v){
		if(isset($_GET[substr($v,0,1)])){
			$$v=$_GET[substr($v,0,1)];
			unset($_GET[substr($v,0,1)]);
		}else if(isset($_SESSION[$v]))$$v=$_SESSION[$v];
		#if($v!="screen"&&$v!="category")unset($_GET[substr($v,0,1)]);
	}
	foreach(array("category","screen") as $v)if(isset($$v)){
		if($$v<0){
			unset($_SESSION[$v]);
			unset($$v);
		}else $_SESSION[$v]=$$v;
	}
	$screen=isset($screen)?$screen:"gantrywall";
	if(isset($halt)){
		addcommand("halt:$screen");
		foreach($_SESSION as $k=>$v)if(preg_match("!^playing-!",$k)&&($screen=="gantrywall"||strpos($k,$screen)!==false))unset($_SESSION[$k]);
		flushcommands($COMMANDFILE);
		exit;
	}else if(isset($play)){
		addcommand("wiedergaben:$screen:$play");
		#$_SESSION["playing"]=$play;
		foreach(array("gantry","wall") as $s)if(strpos($screen,$s)!==false)$_SESSION["playing-$s"]=$play;
		flushcommands($COMMANDFILE);
		exit;
	}
	#if(isset($_GET["s"]))addcommand("screen:$screen");
	#$unlocked=$stage>0?explode(",",file($CATEGORYFILE)[in_array($_GET["v"],$catnames)]):array();
	#$unlocked=isset($category)?explode(",",file($CATEGORYFILE)[array_search($_GET[$category],$catnames)]):array();
	$unlocked=isset($category)?explode(",",trim(file($CATEGORYFILE)[$category])):array();
	sort($unlocked);
	#echo "$unlocked: ".print_r($unlocked,true);
	#$unlocked=isset($_GET["a"])&&$_GET["a"]=="cat"?explode(",",file($CATEGORYFILE)[array_search($_GET["v"],$catnames)]):array();
	#die("Config contents: ".print_r(file($CATEGORYFILE),true));
	#die("Config contents: ".print_r(file($CATEGORYFILE,FILE_INGORE_NEW_LINES),true));
	#die("Unlocked: ".print_r($unlocked,true));
	#die("GET: ".print_r($_GET,true).", query: ".http_build_query($_GET));
	#function findicons($folder,$pattern="",$unlocked=array()){
	function playing($vidname){
		global $screen;
		#return (isset($_SESSION["playing-gantry"])&&$_SESSION["playing-gantry"]==$vidname)||(isset($_SESSION["playing-wall"])&&$_SESSION["playing-wall"]==$vidname)
		#	||(isset($_SESSION["playing-gantrywall"])&&$_SESSION["playing-gantrywall"]==$vidname);
		#foreach($_SESSION as $k=>$v)if(preg_match("!^playing-!",$k)&&($screen=="gantrywall"||strpos($k,$screen)!==false)&&$v==$vidname)return true;
		foreach($_SESSION as $k=>$v)if(preg_match("!^playing-!",$k)&&$v==$vidname)return true;
		return false;
	}
	function findicons($folder,$filter=null){
		global $CATEGORYFILE;
		if(!($d=opendir($folder)))die("\"$folder\" directory not found.");
		$icons=array();
		#while(($entry=readdir($d))!==false)if(($pattern==""||preg_match("!$pattern!",$entry))&&(sizeof($unlocked)==0||array_search(preg_replace("!\..*$!","",$entry),$unlocked)))
		while(($entry=readdir($d))!==false)if(($filter===null||$filter($entry)))$icons[]=$entry;
		return $icons;
	}
	#exit;
	function icongrid($icons,$cols=3){
		#die("Bueller?");
		#die("play: $play, icons: ".print_r($icons,true));
		#$folder="images".(isset($_GET["a"])&&$_GET["a"]=="cat"?"/{$_GET["v"]}":"");
		#global $stage;
		global $category,$play;
		#$folder="images".(isset($_GET["a"])&&$_GET["a"]=="cat"?"/thumbs":"");
		$folder="images/".(isset($category)?"thumbs":"categories");
		echo "<table class='icongrid' id='icongrid'>";
		for($r=0;$r<sizeof($icons)/$cols;$r++){
			echo "<tr>";
			#for($c=0;$c<$cols;$c++)echo "<td><a href='{$_SERVER["PHP_SELF"]}?a=$action&v=".preg_replace("!(^images/|-.*$)!","\\1",$icons[$r*$cols+$c]).
			for($c=0;$c<$cols&&$r*$cols+$c<sizeof($icons);$c++){
				$i=$r*$cols+$c;
				$vidname=preg_replace("![-.].*$!","",$icons[$i]);
				echo "<td id='cell-$vidname'".(playing($vidname)?" class='highlighted'":"").">
					<a ".(isset($category)?"onclick='play(\"$vidname\");":"href='{$_SERVER["PHP_SELF"]}?c=$i")."'>
						<img class='icon' src='$folder/{$icons[$i]}'>
					</a>
				      </td>";
				#echo "<td".(playing($vidname)?" class='highlighted'":"")."><a href='{$_SERVER["PHP_SELF"]}?".
				#	http_build_query((isset($category)?array("p"=>$vidname):array("c"=>$i))+$_GET)."'><img class='icon' src='$folder/{$icons[$i]}'></a></td>";
					#http_build_query(array((isset($category)?"p":"c")=>$i)+$_GET)."'><img class='icon' src='$folder/{$icons[$i]}'></a></td>";

				#echo "<td".(isset($play)&&$play==$vidname?" class='highlighted'":"")."><a href='{$_SERVER["PHP_SELF"]}?c=".(isset($category)?"$category&p=$vidname":$i).
				#	"'><img class='icon' src='$folder/{$icons[$i]}'></a></td>";
				#echo "<td><a href='{$_SERVER["PHP_SELF"]}?c=".(isset($category)?"$category&p=".(isset($play)?"$play":preg_replace("![-.].*$!","",$icons[$i])):$i).
				#echo "<td><a href='{$_SERVER["PHP_SELF"]}?c=".(isset($category)?$category.(isset($play)?"p=$play":""):$i).
				#echo "<td><a href='{$_SERVER["PHP_SELF"]}?c=".($stage+1)."&v=".preg_replace("![-.].*$!","",$icons[$r*$cols+$c]).
				#	"'><img class='icon' src='$folder/{$icons[$r*$cols+$c]}'></a></td>";
			}
			if($c<$cols)echo "<td></td>";
			echo "</tr>";
		}
		echo "</table>";
	}
	function addcommand($command){
		global $commands;
		$commands.=(empty($commands)?"":"\n").$command;
	}
	function flushcommands($COMMANDFILE){
		global $commands,$LOGFILE;
		if(!isset($commands)||empty($commands))return;
		file_put_contents($COMMANDFILE,$commands);
		chmod($COMMANDFILE,0777);
		file_put_contents($LOGFILE,"\n".date("Y-m-d H:i:s").": ---\n$commands",FILE_APPEND);
	}
	flushcommands($COMMANDFILE);
?>
<html>
<head>
<meta http-equiv="Content-Type" content="text/html; charset=UTF-8">
<title>Motions Control Panel</title>
<script language="Javascript">
	function highlight(buttonName){
		var tds=document.getElementById("icongrid").getElementsByTagName("td");
		for(var i=0;i<tds.length;i++)tds[i].className=tds[i].id=="cell-"+buttonName?"highlighted":"";
		//for(var i=0;i<tds.length;i++)tds[i].class=tds[i].getElementsByTagName("img")[0].src.indexOf(buttonName+".png")==tds[i].getElementsByTagName("img")[0].src.length-(buttonName+".png").length
		//	?"highlighted":"";
	}
	function halt(){
		var xhttp=new XMLHttpRequest();
		xhttp.onreadystatechange=function(){
			if(xhttp.readyState==4&&xhttp.status==200&&xhttp.responseText.trim().length>0)alert("Status: "+xhttp.status+"\n\nresponseText (length "+xhttp.responseText.length+"): "+xhttp.responseText);
		};
		xhttp.open("GET","<?=$_SERVER["PHP_SELF"]?>?h",true);
		xhttp.send();
		highlight();
		document.getElementById('haltbutton').style.display="none";
	}
	function play(vidname){
		var xhttp=new XMLHttpRequest();
		xhttp.onreadystatechange=function(){
			if(xhttp.readyState==4&&xhttp.status==200&&xhttp.responseText.trim().length>0)alert("Status: "+xhttp.status+"\n\nresponseText (length "+xhttp.responseText.length+"): "+xhttp.responseText);
		};
		xhttp.open("GET","<?=$_SERVER["PHP_SELF"]?>?p="+vidname.trim(),true);
		xhttp.send();
		highlight(vidname);
		document.getElementById('haltbutton').style.display="block";
	}
</script>
</head>
<style type="text/css">
html{height:100%;}
body{
	margin:0px;
	padding:0px;
	font-family:arial;
	background-image:url("images/background.png");
	background-size:100% 100%;
	overflow:hidden;
	height:100%;
}
div#conainer{
	min-height:100%;
	position:relative;
}
img#header{
	margin-left:auto;
	margin-right:auto;
	position:relative;
	display:block;
	width:100%;
	height:13%;/*118px;*/
	overflow:visible;
	z-index:1;
	padding:0px;
}
img#footer{
	margin-left:auto;
	margin-right:auto;
	display:block;
	position:absolute;
	bottom:0px;
	width:100%;
	height:12%;/*118px;*/
	overflow:visible;
	z-index:1;
}
div#content{
	overflow-x:hidden;
	overflow-y:auto;
	display:block;
	left:0px;
	right:0px;
	position:fixed;
	/*top:13%;
	bottom:12%;*/
	height:75%;
}
img.icon{
	float:left;
	margin:8px;
	width:175px;
	height:175px;
}
img.backarrow{
	position:fixed;
	left:5%;
	bottom:0px;
	height:12%;
	z-index:1;
}
img.stopbutton{
	position:fixed;
	right:5%;
	bottom:0.5%;
	height:10%;
	z-index:1;
}
table.icongrid{
	margin:0% auto;
}
table.icongrid td.highlighted{
	background-image:url("images/thumbhighlight.png");
	background-size:100% 100%;
	background-repeat:no-repeat;
	/*background-color:#FFFF00;*/
}
div#screenmodes{
	text-align:center;
}
/*div#screenmodes>img{
	float:left;
}*/
</style>
<body>
	<div id="container">
	<img id="header" src="images/header.jpg">
	<!--div id="content-wrapper"-->
		<div id="content">
			<?#icongrid(isset($_GET["a"])&&$_GET["a"]=="cat"?findicons("images/{$_GET["v"]}","",$unlocked):findicons("images","-icon.png$"),"cat");?>
			<?
			#die("Unlocked: ".print_r($unlocked,true));
			#icongrid(isset($category)?findicons("images/thumbs",function($e) use ($unlocked){return in_array(preg_replace("!\..*$!","",$e),$unlocked);})
			icongrid(isset($category)?array_map(function($e){return "$e.png";},trim($unlocked[0])==""?array():$unlocked)
						 :array_map(function($e){return "$e.png";},range(0,3)),isset($category)?3:2);
						 #:findicons("images",function($e){return preg_match("!-icon.png!",$e);}));
			#if(true)
			echo "<div id='screenmodes'>";
			$screens=array("gantrywall","gantry","wall");
			$c=0;
			for($i=0;$i<sizeof($screens);$i++)echo "<a href='{$_SERVER["PHP_SELF"]}?".
				http_build_query(array("s"=>$screens[$i])+$_GET)."'><img src='images/screenmode/{$screens[$i]}".(isset($screen)&&$screen==$screens[$i]?"-hl":"").".png'></a>";
			echo "</div>";
			?>
		</div><!--content-->
	<!--/div--><!--content-wrapper-->
	<img id="footer" src="images/<?=(isset($category)?"media":"category")?>.jpg">
	<?
	#echo (isset($category)?"<a href='{$_SERVER["PHP_SELF"]}?".http_build_query(array_diff_key($_GET,array("p"=>"","c"=>"")))."'><img class='backarrow' src='images/backarrowsharp.png'></a>":""
	#echo isset($category)?"<a href='{$_SERVER["PHP_SELF"]}?".http_build_query(array_diff_key($_GET,array("p"=>"","c"=>"")))."'><img class='backarrow' src='images/backarrowsharp.png'></a>":"";
	echo isset($category)?"<a href='{$_SERVER["PHP_SELF"]}?back'><img class='backarrow' src='images/backarrowsharp.png'></a>":"";
	echo "<a id='haltbutton' onclick='halt();'".(isset($_SESSION["playing-$screen"])||($screen=="gantrywall"&&(isset($_SESSION["playing-gantry"])||isset($_SESSION["playing-wall"])))?"":" style='display:none;'")."><img class='stopbutton' src='images/stopbutton.png'></a>";
	#echo isset($_SESSION["playing-$screen"])||($screen=="gantrywall"&&(isset($_SESSION["playing-gantry"])||isset($_SESSION["playing-wall"])))
		#?"<a href='{$_SERVER["PHP_SELF"]}?h'><img class='stopbutton' src='images/stopbutton.png'></a>":"";
	?>
	</div><!--"container"-->
	</body>
</html>
<?#session_end();?>
