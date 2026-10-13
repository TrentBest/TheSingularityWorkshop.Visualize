/**
 * Procedural exoskeleton geometry, silhouette pass 01.
 * +Y up, +Z forward, +X soldier-left. Local coordinates are metres.
 * Vertex format: position.xyz + materialColour.rgb (six float32 values).
 */
const MATERIALS = {
  carbon:[0.035,0.055,0.075], chassis:[0.105,0.15,0.18],
  armor:[0.23,0.31,0.36], edge:[0.42,0.53,0.57],
  ceramic:[0.15,0.22,0.25], joint:[0.075,0.10,0.13],
  light:[0.08,0.88,0.84], warning:[0.95,0.46,0.15],
  visor:[0.025,0.19,0.22], weapon:[0.075,0.09,0.11]
};

/** Stable semantic IDs allow later animation, damage, and detail tiers. */
export const SOLDIER_PARTS = [
 {id:"pelvis.core",shape:"box",at:[0,.91,0],size:[.48,.25,.29],material:"chassis"},
 {id:"leg.left.upper",shape:"box",at:[-.19,.53,0],size:[.22,.4,.24],material:"armor"},
 {id:"leg.right.upper",shape:"box",at:[.19,.53,0],size:[.22,.4,.24],material:"armor"},
 {id:"leg.left.actuator",shape:"box",at:[-.19,.28,0],size:[.17,.52,.19],material:"carbon"},
 {id:"leg.right.actuator",shape:"box",at:[.19,.28,0],size:[.17,.52,.19],material:"carbon"},
 {id:"knee.left.guard",shape:"box",at:[-.19,.08,.07],size:[.23,.13,.34],material:"edge"},
 {id:"knee.right.guard",shape:"box",at:[.19,.08,.07],size:[.23,.13,.34],material:"edge"},
 {id:"boot.left",shape:"box",at:[-.19,.055,.11],size:[.25,.10,.39],material:"ceramic"},
 {id:"boot.right",shape:"box",at:[.19,.055,.11],size:[.25,.10,.39],material:"ceramic"},
 {id:"torso.pressure-frame",shape:"box",at:[0,1.22,0],size:[.56,.48,.31],material:"armor"},
 {id:"torso.sternum-plate",shape:"box",at:[0,1.28,.165],size:[.38,.27,.045],material:"edge"},
 {id:"torso.status-lens",shape:"box",at:[0,1.29,.193],size:[.19,.09,.025],material:"light"},
 {id:"torso.lower-seal",shape:"box",at:[0,1.53,0],size:[.3,.13,.27],material:"carbon"},
 {id:"shoulder.left.hardpoint",shape:"ellipsoid",at:[-.39,1.43,0],radii:[.17,.17,.19],material:"edge"},
 {id:"shoulder.right.hardpoint",shape:"ellipsoid",at:[.39,1.43,0],radii:[.17,.17,.19],material:"edge"},
 {id:"arm.left.upper-shell",shape:"box",at:[-.42,1.19,0],size:[.16,.34,.17],material:"armor"},
 {id:"arm.right.upper-shell",shape:"box",at:[.42,1.19,0],size:[.16,.34,.17],material:"armor"},
 {id:"elbow.left.drive",shape:"ellipsoid",at:[-.43,.98,0],radii:[.095,.10,.11],material:"light"},
 {id:"elbow.right.drive",shape:"ellipsoid",at:[.43,.98,0],radii:[.095,.10,.11],material:"light"},
 {id:"forearm.left.guard",shape:"box",at:[-.43,.83,.01],size:[.15,.22,.16],material:"carbon"},
 {id:"forearm.right.guard",shape:"box",at:[.43,.83,.01],size:[.15,.22,.16],material:"carbon"},
 {id:"pack.life-support",shape:"box",at:[0,1.30,-.23],size:[.36,.45,.20],material:"carbon"},
 {id:"pack.heat-exchangers.left",shape:"box",at:[-.13,1.32,-.35],size:[.07,.34,.07],material:"edge"},
 {id:"pack.heat-exchangers.right",shape:"box",at:[.13,1.32,-.35],size:[.07,.34,.07],material:"edge"},
 {id:"neck.gimbal",shape:"box",at:[0,1.61,.02],size:[.19,.09,.17],material:"joint"},
 {id:"helmet.shell",shape:"ellipsoid",at:[0,1.78,0],radii:[.20,.23,.20],material:"armor",segments:12,rings:8},
 {id:"helmet.brow",shape:"box",at:[0,1.78,.173],size:[.27,.095,.045],material:"carbon"},
 {id:"helmet.visor",shape:"box",at:[0,1.79,.201],size:[.20,.035,.012],material:"visor"},
 {id:"shunt.ocular.left",shape:"box",at:[-.105,1.805,.199],size:[.035,.022,.018],material:"light"},
 {id:"shunt.ocular.right",shape:"box",at:[.105,1.805,.199],size:[.035,.022,.018],material:"light"},
 {id:"shunt.auditory.left",shape:"box",at:[-.198,1.77,0],size:[.025,.07,.10],material:"warning"},
 {id:"shunt.auditory.right",shape:"box",at:[.198,1.77,0],size:[.025,.07,.10],material:"warning"},
 {id:"shunt.olfactory",shape:"box",at:[0,1.735,.192],size:[.026,.028,.018],material:"warning"},
 {id:"shunt.cortical-stem",shape:"box",at:[0,1.59,-.075],size:[.075,.045,.07],material:"warning"},
 {id:"weapon.receiver",shape:"box",at:[.16,1.04,.24],size:[.62,.075,.085],material:"weapon"},
 {id:"weapon.muzzle",shape:"box",at:[.38,1.04,.24],size:[.26,.045,.06],material:"edge"},
 {id:"weapon.grip",shape:"box",at:[-.02,.98,.24],size:[.12,.15,.11],material:"carbon"},
 {id:"weapon.status-lights",shape:"box",at:[.05,1.08,.183],size:[.045,.018,.012],material:"light"}
];

function appendBox(out,center,size,color){
 const [x,y,z]=center,[sx,sy,sz]=size,x0=x-sx/2,x1=x+sx/2,y0=y-sy/2,y1=y+sy/2,z0=z-sz/2,z1=z+sz/2;
 const faces=[
 [[x0,y0,z1],[x1,y0,z1],[x1,y1,z1],[x0,y1,z1]],
 [[x1,y0,z0],[x0,y0,z0],[x0,y1,z0],[x1,y1,z0]],
 [[x0,y0,z0],[x0,y0,z1],[x0,y1,z1],[x0,y1,z0]],
 [[x1,y0,z1],[x1,y0,z0],[x1,y1,z0],[x1,y1,z1]],
 [[x0,y1,z1],[x1,y1,z1],[x1,y1,z0],[x0,y1,z0]],
 [[x0,y0,z0],[x1,y0,z0],[x1,y0,z1],[x0,y0,z1]]];
 for(const face of faces)for(const tri of [[0,1,2],[0,2,3]])for(const j of tri)out.push(...face[j],...color);
}
function appendEllipsoid(out,center,radii,color,segments=10,rings=6){
 const [cx,cy,cz]=center,[rx,ry,rz]=radii;
 const point=(u,v)=>[cx+rx*Math.sin(v)*Math.cos(u),cy+ry*Math.cos(v),cz+rz*Math.sin(v)*Math.sin(u)];
 for(let r=0;r<rings;r++)for(let s=0;s<segments;s++){
  const u=s/segments*Math.PI*2,U=(s+1)/segments*Math.PI*2,v=r/rings*Math.PI,V=(r+1)/rings*Math.PI;
  const p=[point(u,v),point(u,V),point(U,V),point(U,v)];
  for(const j of [0,1,2,0,2,3])out.push(...p[j],...color);
 }
}
export function buildSoldierGeometry(){
 const out=[];
 for(const part of SOLDIER_PARTS){
  const color=MATERIALS[part.material];
  if(part.shape==="box")appendBox(out,part.at,part.size,color);
  else if(part.shape==="ellipsoid")appendEllipsoid(out,part.at,part.radii,color,part.segments??10,part.rings??6);
  else throw new Error("Unsupported geometry primitive: "+part.shape);
 }
 return new Float32Array(out);
}
export const SOLDIER_GEOMETRY_LAYOUT=Object.freeze({
 format:"float32-position3-colour3-nonindexed-triangles",floatsPerVertex:6,bytesPerVertex:24,
 coordinateSystem:"+Y up; +Z forward; +X soldier-left",units:"metres",nominalHeightMetres:1.9,
 partCount:SOLDIER_PARTS.length,
 shuntEndpointIds:["shunt.ocular.left","shunt.ocular.right","shunt.auditory.left","shunt.auditory.right","shunt.olfactory","shunt.cortical-stem"]
});
