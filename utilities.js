$(document).ready(function(){
$('.dropdown-toggle').dropdown();
      
$('.dropdown-menu li a').click(function() {
window.location.assign($(this).attr("href"));
});
});
