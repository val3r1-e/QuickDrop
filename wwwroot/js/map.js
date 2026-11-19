function initQuickDropMap(){
  const c=window.quickDropMap;
  const map=new google.maps.Map(document.getElementById("map"),{center:c.destination,zoom:13,mapTypeControl:false,streetViewControl:false});
  new google.maps.Marker({map,position:c.restaurant,title:"Restaurant"});
  const driver={lat:(c.restaurant.lat+c.destination.lat)/2,lng:(c.restaurant.lng+c.destination.lng)/2};
  const marker=new google.maps.Marker({map,position:driver,title:"Driver",icon:{url:"https://maps.google.com/mapfiles/ms/icons/purple-dot.png"}});
  new google.maps.Marker({map,position:c.destination,title:"Delivery address"});
  let step=0;
  setInterval(()=>{step=(step+1)%101; marker.setPosition({
    lat:c.restaurant.lat+(c.destination.lat-c.restaurant.lat)*step/100,
    lng:c.restaurant.lng+(c.destination.lng-c.restaurant.lng)*step/100
  });},1800);
}