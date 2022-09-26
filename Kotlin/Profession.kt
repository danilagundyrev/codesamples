package com.projectnametemplate.app.data

import androidx.room.Entity
import androidx.room.PrimaryKey

@Entity(tableName = "profession")
data class Profession(
    @PrimaryKey(autoGenerate = false)
    val id: Int,
    val name: String,
    val description: String,
)
