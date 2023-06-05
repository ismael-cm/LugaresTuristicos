function valoracion(idLugar, idStar) {
    const stars = [
        document.getElementById(`star1_${idLugar}`),
        document.getElementById(`star2_${idLugar}`),
        document.getElementById(`star3_${idLugar}`),
        document.getElementById(`star4_${idLugar}`),
        document.getElementById(`star5_${idLugar}`),
    ]


    stars.forEach((star, index) => {
        if (index < idStar)
            star.style.color = '#F9D75D';
        else
            star.style.color = '#5f5050';
    })
}