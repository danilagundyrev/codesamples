package com.projectnametemplate.app.data

import androidx.room.ColumnInfo
import androidx.room.Entity
import androidx.room.PrimaryKey
import com.squareup.moshi.Json

@Entity(tableName = "profile")
data class Profile(
    @PrimaryKey(autoGenerate = false)
    @ColumnInfo(name = "user") @Json(name = "user") val userId: Int,
    val nick: String,
    val email: String,
    val phone: String,
    val status: String,
    val extra: String,
    val professions: List<Int>,
)
