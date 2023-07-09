function MDegrainFft3d(clip c) {
	c
	super = MSuper(pel=2, sharp=1)
	backward_vec3 = MAnalyse(super, isb = true, delta = 3, overlap=4)
	backward_vec2 = MAnalyse(super, isb = true, delta = 2, overlap=4)
	backward_vec1 = MAnalyse(super, isb = true, delta = 1, overlap=4)
	forward_vec1 = MAnalyse(super, isb = false, delta = 1, overlap=4)
	forward_vec2 = MAnalyse(super, isb = false, delta = 2, overlap=4)
	forward_vec3 = MAnalyse(super, isb = false, delta = 3, overlap=4)
	MDegrain3(super, backward_vec1,forward_vec1,backward_vec2,forward_vec2,backward_vec3,forward_vec3,thSAD=400)
	return fft3dfilter(sigma=1.5, bt=5, bw=32, bh=32, ow=16, oh=16)
}