var Astronomy = require('../source/js/astronomy.js');

var date = new Date('1900-01-01T00:00:00Z');
var stop = new Date('2100-01-01T00:00:00Z');
var body, pos, sky, hor;
const observer = Astronomy.MakeObserver(29, -81, 10);

console.log(`o ${observer.latitude} ${observer.longitude} ${observer.height}`);

while (date < stop) {
    for (body of Astronomy.Bodies) {
        if (body !== 'Moon') {
            pos = Astronomy.HelioVector(body, date);
            console.log(`v ${body} ${pos.t.jd_tt} ${pos.x} ${pos.y} ${pos.z}`);
    
            if (body !== 'Earth') {
                pos = Astronomy.GeoVector(body, date);
                sky = Astronomy.SkyPos(pos, observer);
                hor = Astronomy.Horizon(sky.t, observer, sky.ofdate.ra, sky.ofdate.dec);
                console.log(`s ${body} ${pos.t.jd_tt} ${pos.t.jd_utc} ${sky.j2000.ra} ${sky.j2000.dec} ${sky.j2000.dist} ${hor.azimuth} ${hor.altitude}`);
            }
        }
    }
    pos = Astronomy.GeoMoon(date);
    console.log(`v GM ${pos.t.jd_tt} ${pos.x} ${pos.y} ${pos.z}`);

    sky = Astronomy.SkyPos(pos, observer);
    hor = Astronomy.Horizon(sky.t, observer, sky.ofdate.ra, sky.ofdate.dec);
    console.log(`s GM ${pos.t.jd_tt} ${pos.t.jd_utc} ${sky.j2000.ra} ${sky.j2000.dec} ${sky.j2000.dist} ${hor.azimuth} ${hor.altitude}`);

    date = new Date(date.getTime() + (24*3600*1000));
}
